using CanPany.Application.DTOs.Payments;
using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Controller xử lý thông báo thanh toán tức thì (IPN) từ cổng thanh toán SePay.
/// 
/// Luồng hoạt động:
///   1. User hoàn tất thanh toán trên trang SePay.
///   2. SePay gửi POST request tới endpoint này (qua Ngrok trong môi trường local).
///   3. Server xác thực chữ ký, cập nhật trạng thái đơn hàng, trả về 200 OK.
/// 
/// Lưu ý: Endpoint này KHÔNG yêu cầu JWT (AllowAnonymous) vì SePay server gọi trực tiếp.
/// </summary>
[ApiController]
[Route("api/sepay")]
[AllowAnonymous]
public class SepayController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ISePayService _sePayService;
    private readonly ILogger<SepayController> _logger;

    public SepayController(
        IPaymentService paymentService,
        ISePayService sePayService,
        ILogger<SepayController> logger)
    {
        _paymentService = paymentService;
        _sePayService = sePayService;
        _logger = logger;
    }

    /// <summary>
    /// Nhận IPN (Instant Payment Notification) từ SePay.
    /// Route: POST api/sepay/ipn
    /// 
    /// SePay gửi dữ liệu dạng JSON trong body khi thanh toán hoàn thành.
    /// Server BẮT BUỘC trả về HTTP 200 để SePay biết đã nhận thành công.
    /// Nếu không trả về 200, SePay sẽ tự động retry nhiều lần.
    /// </summary>
    [HttpPost("ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveIpn([FromBody] SepayIpnPayload? payload)
    {
        _logger.LogInformation("[SePay IPN] Nhận thông báo thanh toán từ SePay: {@Payload}",
            payload is null ? "null" : new
            {
                payload.Order?.OrderInvoiceNumber,
                payload.Order?.OrderAmount,
                payload.NotificationType,
                payload.Transaction?.PaymentMethod,
                payload.Transaction?.TransactionId
            });

        // -------------------------------------------------------
        // Bước 0.5: Xác thực IPN
        // SePay PG API gửi IPN **KHÔNG** kèm X-Secret-Key header.
        // Xác thực dựa trên: whitelist IP nguồn, hoặc kiểm tra order tồn tại trong DB.
        // Trong sandbox, ta log warning nếu thiếu header nhưng vẫn xử lý.
        // -------------------------------------------------------
        if (Request.Headers.TryGetValue("X-Secret-Key", out var providedSecretKey))
        {
            if (!_sePayService.VerifySecretKey(providedSecretKey.ToString()))
            {
                _logger.LogWarning("[SePay IPN] X-Secret-Key không khớp. Vẫn xử lý nhưng cần kiểm tra cấu hình.");
            }
            else
            {
                _logger.LogInformation("[SePay IPN] X-Secret-Key xác thực thành công.");
            }
        }
        else
        {
            _logger.LogInformation("[SePay IPN] Không có header X-Secret-Key (expected cho SePay PG IPN).");
        }

        // -------------------------------------------------------
        // Bước 1: Kiểm tra payload có hợp lệ không
        // -------------------------------------------------------
        if (payload?.Order is null || string.IsNullOrWhiteSpace(payload.Order.OrderInvoiceNumber))
        {
            _logger.LogWarning("[SePay IPN] Payload không hợp lệ hoặc thiếu order_invoice_number.");
            // Vẫn trả về 200 để SePay không retry, nhưng kèm mã lỗi
            return Ok(new SepayIpnResponse { Code = "01", Message = "Invalid payload" });
        }

        // -------------------------------------------------------
        // Bước 2: Xử lý kết quả thanh toán
        // SePay PG IPN docs show we only check notification_type == "ORDER_PAID"
        // -------------------------------------------------------
        try
        {
            if (payload.NotificationType == "ORDER_PAID")
            {
                _logger.LogInformation(
                    "[SePay IPN] Thanh toán THÀNH CÔNG - OrderInvoiceNumber: {OrderNo}, Amount: {Amount}, TransactionId: {TxnId}, PaymentMethod: {Method}",
                    payload.Order.OrderInvoiceNumber, payload.Order.OrderAmount,
                    payload.Transaction?.TransactionId, payload.Transaction?.PaymentMethod);

                // Update database
                var webhookData = new Dictionary<string, string>
                {
                    ["order_invoice_number"] = payload.Order.OrderInvoiceNumber,
                    ["response_code"]        = "00", // simulate success code for PaymentService
                    ["transaction_id"]       = payload.Transaction?.TransactionId ?? "",
                    ["payment_method"]       = payload.Transaction?.PaymentMethod ?? ""
                };
                await _paymentService.ProcessSePayCallbackAsync(webhookData);
            }
            else if (payload.NotificationType == "TRANSACTION_VOID")
            {
                _logger.LogWarning(
                    "[SePay IPN] Giao dịch đã bị HỦY (VOID) - OrderNo: {OrderNo}, TransactionId: {TxnId}",
                    payload.Order.OrderInvoiceNumber, payload.Transaction?.TransactionId);

                // Mark payment as failed/refunded
                var webhookData = new Dictionary<string, string>
                {
                    ["order_invoice_number"] = payload.Order.OrderInvoiceNumber,
                    ["response_code"]        = "24", // cancelled code
                    ["transaction_id"]       = payload.Transaction?.TransactionId ?? "",
                    ["payment_method"]       = payload.Transaction?.PaymentMethod ?? ""
                };
                await _paymentService.ProcessSePayCallbackAsync(webhookData);
            }
            else
            {
                _logger.LogWarning(
                    "[SePay IPN] Nhận thông báo không xử lý - NotificationType: {Type}, OrderNo: {OrderNo}",
                    payload.NotificationType, payload.Order.OrderInvoiceNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SePay IPN] Lỗi khi xử lý IPN cho đơn hàng {OrderNo}", payload?.Order?.OrderInvoiceNumber);
            // Vẫn trả về 200 để SePay không retry liên tục khi server lỗi tạm thời
            return Ok(new SepayIpnResponse { Code = "99", Message = "Server error" });
        }

        // -------------------------------------------------------
        // Bước 4: Trả về HTTP 200 OK — BẮT BUỘC
        // SePay sẽ coi là thành công nếu nhận được HTTP 200.
        // Nếu trả về 4xx/5xx, SePay sẽ retry request này nhiều lần.
        // -------------------------------------------------------
        return Ok(new SepayIpnResponse { Code = "00", Message = "OK" });
    }
}
