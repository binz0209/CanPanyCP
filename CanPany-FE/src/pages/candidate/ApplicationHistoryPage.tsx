import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  Briefcase, 
  MapPin, 
  Clock, 
  DollarSign, 
  Building2, 
  X,
  AlertCircle,
  CheckCircle,
  XCircle,
  Hourglass,
  FileText
} from 'lucide-react';
import toast from 'react-hot-toast';
import { Button } from '../../components/ui/Button';
import { applicationsApi } from '../../api/applications.api';
import type { Application, ApplicationStatus } from '../../types/application.types';
import { cn } from '../../utils';

const statusConfig: Record<ApplicationStatus, { 
  label: string; 
  color: string; 
  bgColor: string;
  icon: React.ReactNode 
}> = {
  Pending: {
    label: 'Chờ xét duyệt',
    color: 'text-amber-600',
    bgColor: 'bg-amber-50',
    icon: <Hourglass className="h-4 w-4" />
  },
  Shortlisted: {
    label: 'Vào vòng tiếp theo',
    color: 'text-blue-600',
    bgColor: 'bg-blue-50',
    icon: <CheckCircle className="h-4 w-4" />
  },
  Accepted: {
    label: 'Đã chấp nhận',
    color: 'text-green-600',
    bgColor: 'bg-green-50',
    icon: <CheckCircle className="h-4 w-4" />
  },
  Rejected: {
    label: 'Bị từ chối',
    color: 'text-red-600',
    bgColor: 'bg-red-50',
    icon: <XCircle className="h-4 w-4" />
  },
  Withdrawn: {
    label: 'Đã rút đơn',
    color: 'text-gray-600',
    bgColor: 'bg-gray-50',
    icon: <X className="h-4 w-4" />
  }
};

export function ApplicationHistoryPage() {
  const navigate = useNavigate();
  const [applications, setApplications] = useState<Application[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [withdrawingId, setWithdrawingId] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<ApplicationStatus | 'All'>('All');
  const [showConfirm, setShowConfirm] = useState(false);
  const [confirmingApplicationId, setConfirmingApplicationId] = useState<string | null>(null);

  useEffect(() => {
    loadApplications();
  }, []);

  const loadApplications = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await applicationsApi.getMyApplications();
      setApplications(data);
    } catch (err) {
      setError('Failed to load applications. Please try again.');
      console.error('Error loading applications:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleWithdraw = async (applicationId: string) => {
    setConfirmingApplicationId(applicationId);
    setShowConfirm(true);
  };

  const confirmWithdraw = async () => {
    if (!confirmingApplicationId) return;

    try {
      setWithdrawingId(confirmingApplicationId);
      await applicationsApi.withdraw(confirmingApplicationId);
      await loadApplications();
      toast.success('Đơn đã rút', {
        duration: 2000,
        position: 'top-right',
      });
    } catch (err) {
      toast.error('Không thể rút đơn. Vui lòng thử lại.');
      console.error('Error withdrawing application:', err);
    } finally {
      setWithdrawingId(null);
      setShowConfirm(false);
      setConfirmingApplicationId(null);
    }
  };

  const cancelWithdraw = () => {
    setShowConfirm(false);
    setConfirmingApplicationId(null);
  };

  // Filter applications based on selected status
  const filteredApplications = filterStatus === 'All' 
    ? applications 
    : applications.filter(app => app.status === filterStatus);

  // Check if withdraw is allowed (only on Pending status)
  const canWithdraw = (status: ApplicationStatus): boolean => {
    return status === 'Pending';
  };

  // Calculate statistics
  const stats = {
    total: applications.length,
    pending: applications.filter(a => a.status === 'Pending').length,
    shortlisted: applications.filter(a => a.status === 'Shortlisted').length,
    accepted: applications.filter(a => a.status === 'Accepted').length,
    rejected: applications.filter(a => a.status === 'Rejected').length,
    withdrawn: applications.filter(a => a.status === 'Withdrawn').length,
  };

  const formatDate = (date: string | Date) => {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const formatSalary = (amount?: number) => {
    if (!amount) return 'Not specified';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0
    }).format(amount);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#00b14f]"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px] gap-4">
        <AlertCircle className="h-12 w-12 text-red-500" />
        <p className="text-gray-600">{error}</p>
        <Button onClick={loadApplications}>Thử lại</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Lịch sử ứng tuyển</h1>
          <p className="text-gray-600 mt-1">Theo dõi và quản lý các đơn ứng tuyển của bạn</p>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="text-2xl font-bold text-gray-900">{stats.total}</div>
          <div className="text-sm text-gray-500">Tổng số</div>
        </div>
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="text-2xl font-bold text-amber-600">{stats.pending}</div>
          <div className="text-sm text-gray-500">Chờ duyệt</div>
        </div>
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="text-2xl font-bold text-blue-600">{stats.shortlisted}</div>
          <div className="text-sm text-gray-500">Vào vòng tiếp</div>
        </div>
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="text-2xl font-bold text-green-600">{stats.accepted}</div>
          <div className="text-sm text-gray-500">Đã chấp nhận</div>
        </div>
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="text-2xl font-bold text-red-600">{stats.rejected}</div>
          <div className="text-sm text-gray-500">Bị từ chối</div>
        </div>
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="text-2xl font-bold text-gray-600">{stats.withdrawn}</div>
          <div className="text-sm text-gray-500">Đã rút đơn</div>
        </div>
      </div>

      {/* Filter */}
      <div className="flex items-center gap-2 flex-wrap">
        <span className="text-sm text-gray-600">Lọc theo trạng thái:</span>
        {(['All', 'Pending', 'Shortlisted', 'Accepted', 'Rejected', 'Withdrawn'] as const).map((status) => {
          const labelMap: Record<string, string> = {
            All: 'Tất cả',
            Pending: 'Chờ duyệt',
            Shortlisted: 'Vào vòng tiếp',
            Accepted: 'Đã chấp nhận',
            Rejected: 'Bị từ chối',
            Withdrawn: 'Đã rút đơn',
          };
          return (
            <Button
              key={status}
              variant={filterStatus === status ? 'default' : 'outline'}
              size="sm"
              onClick={() => setFilterStatus(status)}
              className={cn(
                filterStatus === status && 'bg-[#00b14f] hover:bg-[#00a048] border-[#00b14f]'
              )}
            >
              {labelMap[status]}
            </Button>
          );
        })}
      </div>

      {/* Applications List */}
      {filteredApplications.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 gap-4 bg-white rounded-lg border border-gray-200">
          <FileText className="h-12 w-12 text-gray-400" />
          <p className="text-gray-600">
            {filterStatus === 'All' 
              ? 'Bạn chưa ứng tuyển vào công việc nào.' 
              : `Không có đơn ứng tuyển ở trạng thái này.`
            }
          </p>
          <Button 
            onClick={() => navigate('/jobs')}
            className="bg-[#00b14f] hover:bg-[#00a048]"
          >
            Khám phá việc làm
          </Button>
        </div>
      ) : (
        <div className="space-y-4">
          {filteredApplications.map((application) => {
            const status = statusConfig[application.status];
            const withdrawAllowed = canWithdraw(application.status);
            
            return (
              <div 
                key={application.id}
                className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-md transition-shadow"
              >
                <div className="flex flex-col md:flex-row md:items-start justify-between gap-4">
                  {/* Job Info */}
                  <div className="flex-1">
                    <div className="flex items-start gap-4">
                      <div className="w-12 h-12 rounded-lg bg-gray-100 flex items-center justify-center flex-shrink-0 overflow-hidden">
                        {application.job?.company?.logoUrl ? (
                          <img 
                            src={application.job.company.logoUrl} 
                            alt={application.job.company.name}
                            className="w-full h-full object-cover"
                          />
                        ) : (
                          <Building2 className="h-6 w-6 text-gray-400" />
                        )}
                      </div>
                      <div className="flex-1 min-w-0">
                        <h3 className="text-lg font-semibold text-gray-900">
                          {application.job?.title || 'Vị trí tuyển dụng'}
                        </h3>
                        <div className="flex flex-wrap items-center gap-3 mt-1 text-sm text-gray-600">
                          {application.job?.company && (
                            <span className="flex items-center gap-1">
                              <Building2 className="h-4 w-4" />
                              {application.job.company.name}
                            </span>
                          )}
                          {application.job?.location && (
                            <span className="flex items-center gap-1">
                              <MapPin className="h-4 w-4" />
                              {application.job.location}
                            </span>
                          )}
                          {application.job?.isRemote && (
                            <span className="px-2 py-0.5 bg-green-50 text-green-700 rounded-full text-xs">
                              Remote
                            </span>
                          )}
                        </div>
                      </div>
                    </div>

                    {/* Application Details */}
                    <div className="mt-4 flex flex-wrap gap-4 text-sm">
                      {application.proposedAmount && (
                        <span className="flex items-center gap-1 text-gray-600">
                          <DollarSign className="h-4 w-4" />
                          Mức đề xuất: {formatSalary(application.proposedAmount)}
                        </span>
                      )}
                      <span className="flex items-center gap-1 text-gray-600">
                        <Clock className="h-4 w-4" />
                        Nộp đơn: {formatDate(application.createdAt)}
                      </span>
                      {application.matchScore !== undefined && (
                        <span className="flex items-center gap-1 text-gray-600">
                          <Briefcase className="h-4 w-4" />
                          Phù hợp: {Math.round(application.matchScore)}%
                        </span>
                      )}
                    </div>

                    {/* Cover Letter Preview */}
                    {application.coverLetter && (
                      <div className="mt-3 p-3 bg-gray-50 rounded-lg">
                        <p className="text-sm text-gray-600 line-clamp-2">
                          {application.coverLetter}
                        </p>
                      </div>
                    )}
                  </div>

                  {/* Status & Actions */}
                  <div className="flex flex-col items-end gap-3">
                    <div className={cn(
                      'inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-sm font-medium',
                      status.bgColor,
                      status.color
                    )}>
                      {status.icon}
                      {status.label}
                    </div>

                    {/* Withdraw Button - Only show before pending status */}
                    {withdrawAllowed && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleWithdraw(application.id)}
                        disabled={withdrawingId === application.id}
                        className="text-red-600 border-red-200 hover:bg-red-50 hover:border-red-300"
                      >
                        {withdrawingId === application.id ? (
                          <span className="flex items-center gap-1">
                            <div className="animate-spin rounded-full h-3 w-3 border-b-2 border-red-600"></div>
                            Đang rút...
                          </span>
                        ) : (
                          <span className="flex items-center gap-1">
                            <X className="h-4 w-4" />
                            Rút đơn
                          </span>
                        )}
                      </Button>
                    )}

                    {/* Status Message */}
                    {!withdrawAllowed && (
                      <p className="text-xs text-gray-500 text-right">
                        {application.status === 'Shortlisted' && 'Chúc mừng, bạn đã vào vòng tiếp theo!'}
                        {application.status === 'Accepted' && 'Chúc mừng, đơn ứng tuyển đã được chấp nhận!'}
                        {application.status === 'Rejected' && 'Hồ sơ chưa phù hợp với vị trí này'}
                        {application.status === 'Withdrawn' && 'Bạn đã rút đơn ứng tuyển này'}
                      </p>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Confirm Withdraw Modal */}
      {showConfirm && (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-2xl max-w-sm w-full mx-4 p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Rút đơn ứng tuyển</h3>
            <p className="text-gray-600 mb-6">Bạn có chắc chắn muốn rút đơn ứng tuyển của mình không? Hành động này không thể hoàn tác.</p>
            <div className="flex gap-3">
              <button
                onClick={cancelWithdraw}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Không
              </button>
              <button
                onClick={confirmWithdraw}
                disabled={withdrawingId === confirmingApplicationId}
                className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-red-400 text-white rounded-lg transition-colors"
              >
                {withdrawingId === confirmingApplicationId ? 'Đang rút...' : 'Có, rút ngay'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default ApplicationHistoryPage;
