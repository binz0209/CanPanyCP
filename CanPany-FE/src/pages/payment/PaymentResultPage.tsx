import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { CheckCircle, XCircle, AlertCircle, ArrowLeft, Wallet } from 'lucide-react';
import { Button, Card, CardContent } from '../../components/ui';

export function PaymentResultPage() {
  const { status } = useParams<{ status: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation('candidate');
  const [countdown, setCountdown] = useState(5);

  useEffect(() => {
    const timer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          navigate('/candidate/wallet', { replace: true });
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, [navigate]);

  let icon = <CheckCircle className="h-16 w-16 text-green-500 mx-auto mb-4" />;
  let title = t('paymentResult.success.title');
  let message = t('paymentResult.success.message');
  let color = 'bg-green-50 border-green-200';

  if (status === 'error') {
    icon = <XCircle className="h-16 w-16 text-red-500 mx-auto mb-4" />;
    title = t('paymentResult.error.title');
    message = t('paymentResult.error.message');
    color = 'bg-red-50 border-red-200';
  } else if (status === 'cancel') {
    icon = <AlertCircle className="h-16 w-16 text-yellow-500 mx-auto mb-4" />;
    title = t('paymentResult.cancel.title');
    message = t('paymentResult.cancel.message');
    color = 'bg-yellow-50 border-yellow-200';
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <Card className={`max-w-md w-full border ${color} shadow-lg`}>
        <CardContent className="pt-8 pb-8 px-6 text-center">
          {icon}
          <h1 className="text-2xl font-bold text-gray-900 mb-2">{title}</h1>
          <p className="text-gray-600 mb-8">{message}</p>
          
          <div className="space-y-3">
            <Button 
              className="w-full bg-[#00b14f] hover:bg-[#00953f] text-white flex items-center justify-center gap-2"
              onClick={() => navigate('/candidate/wallet', { replace: true })}
            >
              <Wallet className="h-4 w-4" />
              {t('paymentResult.goToWallet')}
            </Button>
            <Button 
              variant="outline"
              className="w-full flex items-center justify-center gap-2"
              onClick={() => navigate('/', { replace: true })}
            >
              <ArrowLeft className="h-4 w-4" />
              {t('paymentResult.goHome')}
            </Button>
          </div>
          
          <p className="text-xs text-gray-400 mt-6">
            {t('paymentResult.autoRedirect', { seconds: countdown })}
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
