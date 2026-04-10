import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { ChevronRight, ArrowRight, Briefcase, Bookmark, FileText, Clock, CheckCircle, XCircle, Loader2, Star } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { candidateApi } from '../../api/candidate.api';
import type { CandidateStatistics } from '../../api/candidate.api';
import { applicationsApi } from '../../api/applications.api';
import { jobsApi } from '../../api/jobs.api';
import { companiesApi } from '../../api/companies.api';
import { useAuthStore } from '../../stores/auth.store';
import type { Application } from '../../types';
import type { RecommendedJob, Job } from '../../types/job.types';
import { useTranslation } from 'react-i18next';

type ApplicationStatus = 'Pending' | 'Shortlisted' | 'Accepted' | 'Rejected' | 'Withdrawn';

export function CandidateDashboardPage() {
  const { t } = useTranslation('candidate');
  const navigate = useNavigate();
  const { user } = useAuthStore();
    const STATUS_CONFIG: Record<ApplicationStatus, { label: string; className: string }> = {
      Pending:     { label: t('dashboard.status.pending'),     className: 'bg-blue-100 text-blue-700' },
      Shortlisted: { label: t('dashboard.status.shortlisted'), className: 'bg-yellow-100 text-yellow-700' },
      Accepted:    { label: t('dashboard.status.accepted'),    className: 'bg-green-100 text-green-700' },
      Rejected:    { label: t('dashboard.status.rejected'),    className: 'bg-red-100 text-red-700' },
      Withdrawn:   { label: t('dashboard.status.withdrawn'),   className: 'bg-gray-100 text-gray-700' },
    };

    const formatRelativeDate = (date: string | Date): string => {
      const diffDays = Math.floor((Date.now() - new Date(date).getTime()) / 86_400_000);
      if (diffDays === 0) return t('dashboard.relative.today');
      if (diffDays === 1) return t('dashboard.relative.yesterday');
      if (diffDays < 7) return t('dashboard.relative.days', { count: diffDays });
      if (diffDays < 14) return t('dashboard.relative.oneWeek');
      if (diffDays < 30) return t('dashboard.relative.weeks', { count: Math.floor(diffDays / 7) });
      return t('dashboard.relative.months', { count: Math.floor(diffDays / 30) });
    };

    const formatBudget = (job: Job): string => {
      if (!job.budgetAmount) return t('dashboard.budget.negotiable');
      const amount = job.budgetAmount.toLocaleString('en-US');
      return job.budgetType === 'Hourly'
        ? `${t('dashboard.budget.prefix', { amount })}${t('dashboard.budget.perHour')}`
        : t('dashboard.budget.prefix', { amount });
    };

  const [statistics, setStatistics] = useState<CandidateStatistics | null>(null);
  const [recentApplications, setRecentApplications] = useState<Application[]>([]);
  const [recommendedJobs, setRecommendedJobs] = useState<RecommendedJob[]>([]);
  const [bookmarkCount, setBookmarkCount] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      if (!user?.id) {
        setLoading(false);
        return;
      }

      setLoading(true);
      const [statsResult, applicationsResult, recommendedResult, bookmarkedResult] = await Promise.allSettled([
        candidateApi.getCandidateStatistics(user.id),
        applicationsApi.getMyApplications(),
        jobsApi.getRecommended(3),
        jobsApi.getBookmarked(),
      ]);

      if (statsResult.status === 'fulfilled') {
        setStatistics(statsResult.value);
      } else {
        console.error('Failed to fetch statistics:', statsResult.reason);
        setStatistics({
          totalApplications: 0, pendingApplications: 0,
          acceptedApplications: 0, rejectedApplications: 0,
          totalCVs: 0, profileCompleteness: 0, skillsCount: 0,
        });
      }

      if (applicationsResult.status === 'fulfilled') {
        const baseApplications = applicationsResult.value;
        const baseRecommended = recommendedResult.status === 'fulfilled' ? recommendedResult.value : [];

        const uniqueJobIds = Array.from(
          new Set(
            [
              ...baseApplications.map((app) => app.jobId).filter(Boolean),
              ...baseRecommended.map((item) => item.job?.id).filter(Boolean),
            ]
          )
        ) as string[];

        const jobsById = new Map<string, Job>();
        if (uniqueJobIds.length > 0) {
          const jobResponses = await Promise.allSettled(uniqueJobIds.map((jobId) => jobsApi.getById(jobId)));
          jobResponses.forEach((result, index) => {
            if (result.status === 'fulfilled' && result.value?.job) {
              jobsById.set(uniqueJobIds[index], result.value.job);
            }
          });

          const uniqueCompanyIds = Array.from(
            new Set(
              Array.from(jobsById.values())
                .filter((job) => !job.company && Boolean(job.companyId))
                .map((job) => job.companyId)
            )
          );

          if (uniqueCompanyIds.length > 0) {
            const companyResponses = await Promise.allSettled(
              uniqueCompanyIds.map((companyId) => companiesApi.getById(companyId))
            );
            const companiesById = new Map<string, Awaited<ReturnType<typeof companiesApi.getById>>>();

            companyResponses.forEach((result, index) => {
              if (result.status === 'fulfilled' && result.value) {
                companiesById.set(uniqueCompanyIds[index], result.value);
              }
            });

            jobsById.forEach((job, jobId) => {
              if (!job.company && job.companyId) {
                const company = companiesById.get(job.companyId);
                if (company) {
                  jobsById.set(jobId, {
                    ...job,
                    company,
                  });
                }
              }
            });
          }
        }

        const hydratedApplications = baseApplications.map((app) => ({
          ...app,
          job: app.job ?? jobsById.get(app.jobId),
        }));

        const sorted = [...hydratedApplications].sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        setRecentApplications(sorted.slice(0, 3));

        if (recommendedResult.status === 'fulfilled') {
          const hydratedRecommended = recommendedResult.value.map((item) => ({
            ...item,
            job: jobsById.get(item.job?.id) ?? item.job,
          }));
          setRecommendedJobs(hydratedRecommended);
        }
      } else {
        console.error('Failed to fetch applications:', applicationsResult.reason);
      }

      if (recommendedResult.status === 'fulfilled' && applicationsResult.status !== 'fulfilled') {
        setRecommendedJobs(recommendedResult.value);
      } else {
        if (recommendedResult.status !== 'fulfilled') {
          console.error('Failed to fetch recommended jobs:', recommendedResult.reason);
        }
      }

      if (bookmarkedResult.status === 'fulfilled') {
        setBookmarkCount(bookmarkedResult.value.length);
      } else {
        console.error('Failed to fetch bookmarks:', bookmarkedResult.reason);
      }

      setLoading(false);
    };

    fetchData();
  }, [user?.id]);

  const cardAnimation = { animation: 'fadeSlideUp 0.5s ease-out forwards', opacity: 0 };

  return (
    <div className="min-h-screen">
      {/* Breadcrumb */}
      <div className="mb-8 flex items-center gap-2 text-sm text-gray-600">
        <span>{t('dashboard.breadcrumb.dashboard')}</span>
        <ChevronRight className="h-4 w-4" />
        <span className="text-gray-900 font-medium">{t('dashboard.breadcrumb.overview')}</span>
      </div>

      {/* Page Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">{t('dashboard.header.title')}</h1>
        <p className="text-gray-600">{t('dashboard.header.subtitle')}</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        {/* Total Applications */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.1s' }}>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <Briefcase className="h-4 w-4 text-[#00b14f]" />
              {t('dashboard.stats.totalApplications.title')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-12">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : (
              <>
                <div className="text-3xl font-bold text-gray-900">{statistics?.totalApplications ?? '-'}</div>
                <p className="text-xs text-gray-600 mt-1">
                  {t('dashboard.stats.totalApplications.meta', {
                    pending: statistics?.pendingApplications ?? 0,
                    accepted: statistics?.acceptedApplications ?? 0,
                  })}
                </p>
              </>
            )}
          </CardContent>
        </Card>

        {/* Bookmarked Jobs */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.2s' }}>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <Bookmark className="h-4 w-4 text-[#00b14f]" />
              {t('dashboard.stats.bookmarked.title')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-12">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : (
              <>
                <div className="text-3xl font-bold text-gray-900">{bookmarkCount}</div>
                <p className="text-xs text-gray-600 mt-1">{t('dashboard.stats.bookmarked.meta')}</p>
              </>
            )}
          </CardContent>
        </Card>

        {/* Skills Count */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.3s' }}>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <Star className="h-4 w-4 text-[#00b14f]" />
              {t('dashboard.stats.skills.title')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-12">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : (
              <>
                <div className="text-3xl font-bold text-gray-900">{statistics?.skillsCount ?? 0}</div>
                <p className="text-xs text-gray-600 mt-1">{t('dashboard.stats.skills.meta')}</p>
              </>
            )}
          </CardContent>
        </Card>

        {/* Total CVs */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.4s' }}>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <FileText className="h-4 w-4 text-[#00b14f]" />
              {t('dashboard.stats.cvs.title')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-12">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : (
              <>
                <div className="text-3xl font-bold text-gray-900">{statistics?.totalCVs ?? '-'}</div>
                <p className="text-xs text-gray-600 mt-1">
                  {statistics?.defaultCV?.fileName
                    ? t('dashboard.stats.cvs.metaDefault', { fileName: statistics.defaultCV.fileName })
                    : t('dashboard.stats.cvs.metaEmpty')}
                </p>
              </>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Application Status Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.5s' }}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-blue-100 rounded-full">
                <Clock className="h-6 w-6 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-gray-600">{t('dashboard.status.pending')}</p>
                <p className="text-2xl font-bold text-gray-900">{loading ? '-' : statistics?.pendingApplications ?? 0}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.6s' }}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-green-100 rounded-full">
                <CheckCircle className="h-6 w-6 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-gray-600">{t('dashboard.status.accepted')}</p>
                <p className="text-2xl font-bold text-gray-900">{loading ? '-' : statistics?.acceptedApplications ?? 0}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.7s' }}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-red-100 rounded-full">
                <XCircle className="h-6 w-6 text-red-600" />
              </div>
              <div>
                <p className="text-sm text-gray-600">{t('dashboard.status.rejected')}</p>
                <p className="text-2xl font-bold text-gray-900">{loading ? '-' : statistics?.rejectedApplications ?? 0}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Profile Completeness */}
      <Card className="mb-8 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.8s' }}>
        <CardHeader>
          <CardTitle className="text-lg">{t('dashboard.profile.title')}</CardTitle>
          <CardDescription>{t('dashboard.profile.description')}</CardDescription>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex items-center justify-center h-8">
              <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
            </div>
          ) : (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">
                  {t('dashboard.profile.level', { percent: statistics?.profileCompleteness ?? 0 })}
                </span>
                <span className="text-sm text-gray-500">{t('dashboard.profile.skills', { count: statistics?.skillsCount ?? 0 })}</span>
              </div>
              <div className="w-full h-4 bg-gray-200 rounded-full overflow-hidden">
                <div
                  className="h-full bg-linear-to-r from-[#00b14f] to-[#00d463] rounded-full transition-all duration-1000 ease-out"
                  style={{ width: `${statistics?.profileCompleteness ?? 0}%` }}
                />
              </div>
              {(statistics?.profileCompleteness ?? 0) < 100 && (
                <p className="text-xs text-gray-500">{t('dashboard.profile.cta')}</p>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Applications */}
        <Card className="lg:col-span-2 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.9s' }}>
          <CardHeader>
            <CardTitle>{t('dashboard.recent.title')}</CardTitle>
            <CardDescription>{t('dashboard.recent.description')}</CardDescription>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-24">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : recentApplications.length === 0 ? (
              <p className="text-sm text-gray-500 text-center py-6">{t('dashboard.recent.empty')}</p>
            ) : (
              <div className="space-y-4">
                {recentApplications.map((app) => {
                  const statusCfg = STATUS_CONFIG[app.status as ApplicationStatus] ?? { label: app.status, className: 'bg-gray-100 text-gray-700' };
                  return (
                    <div key={app.id} className="flex items-center justify-between p-4 rounded-lg border border-gray-200 hover:bg-gray-50 hover:shadow-md transition-all duration-300">
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-gray-900 text-sm truncate">
                          {app.job?.title ?? t('dashboard.recent.unknownPosition')}
                        </p>
                        <p className="text-xs text-gray-600">
                          {app.job?.company?.name ?? t('dashboard.recent.unknownCompany')} · {formatRelativeDate(app.createdAt)}
                        </p>
                      </div>
                      <span className={`ml-3 shrink-0 text-xs font-semibold px-2 py-1 rounded-full ${statusCfg.className}`}>
                        {statusCfg.label}
                      </span>
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '1.0s' }}>
          <CardHeader>
            <CardTitle>{t('dashboard.quick.title')}</CardTitle>
            <CardDescription>{t('dashboard.quick.description')}</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            <Button
              className="w-full justify-between bg-[#00b14f] hover:bg-[#00a045] text-white"
              onClick={() => navigate('/jobs')}
            >
              <span>{t('dashboard.quick.findJobs')}</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              className="w-full justify-between border-gray-300 bg-transparent hover:bg-[#00b14f] hover:text-white"
              onClick={() => navigate('/candidate/profile')}
            >
              <span>{t('dashboard.quick.profile')}</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              className="w-full justify-between border-gray-300 bg-transparent hover:bg-[#00b14f] hover:text-white"
              onClick={() => navigate('/candidate/cv/list')}
            >
              <span>{t('dashboard.quick.uploadCv')}</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              className="w-full justify-between border-gray-300 bg-transparent hover:bg-[#00b14f] hover:text-white"
              onClick={() => navigate('/candidate/jobs/recommended')}
            >
              <span>{t('dashboard.quick.seeRecommendations')}</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Recommended Jobs */}
      <Card className="mt-6 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '1.1s' }}>
        <CardHeader>
          <CardTitle>{t('dashboard.recommended.title')}</CardTitle>
          <CardDescription>{t('dashboard.recommended.description')}</CardDescription>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex items-center justify-center h-24">
              <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
            </div>
            ) : recommendedJobs.length === 0 ? (
            <p className="text-sm text-gray-500 text-center py-6">
              {t('dashboard.recommended.empty')}
            </p>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {recommendedJobs.map(({ job }) => (
                <div
                  key={job.id}
                  className="flex flex-col gap-3 p-4 rounded-lg border border-gray-200 hover:border-[#00b14f] hover:bg-gray-50 hover:shadow-lg transition-all duration-300 group cursor-pointer"
                >
                  <div>
                    <p className="font-semibold text-gray-900 text-sm group-hover:text-[#00b14f] transition-colors truncate">
                      {job.title}
                    </p>
                    <p className="text-xs text-gray-600 truncate">{job.company?.name ?? t('dashboard.recent.unknownCompany')}</p>
                  </div>
                  <div className="space-y-1 text-xs text-gray-600">
                    {(job.location || job.isRemote) && (
                      <p>📍 {job.isRemote ? t('dashboard.recommended.remote') : job.location}</p>
                    )}
                    <p>💰 {formatBudget(job)}</p>
                  </div>
                  <Button
                    size="sm"
                    variant="outline"
                    className="w-full text-xs border-gray-300 hover:border-[#00b14f] bg-transparent"
                    onClick={() => navigate(`/jobs/${job.id}`)}
                  >
                    {t('dashboard.recommended.viewDetail')}
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
