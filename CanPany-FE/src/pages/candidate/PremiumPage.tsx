import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import {
  Crown, Check, Sparkles, FileText, Search, Eye,
  ArrowRight, Wallet, Clock, Shield
} from 'lucide-react';
import { Button, Card, CardContent, CardHeader, CardTitle, Badge } from '../../components/ui';
import { paymentsApi, type PremiumPackage } from '../../api/payments.api';
import { walletApi } from '../../api/wallet.api';
import { useAuthStore } from '../../stores/auth.store';
import { formatCurrency, formatDateTime } from '../../utils';

export function PremiumPage() {
  const { token } = useAuthStore();
  const { t } = useTranslation('candidate');
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [selectedPackageId, setSelectedPackageId] = useState<string | null>(null);

  // Fetch premium status
  const statusQuery = useQuery({
    queryKey: ['premium', 'status'],
    enabled: !!token,
    queryFn: () => paymentsApi.getPremiumStatus(),
  });

  // Fetch packages
  const packagesQuery = useQuery({
    queryKey: ['premium', 'packages'],
    enabled: !!token,
    queryFn: () => paymentsApi.getPremiumPackages(),
  });

  // Fetch wallet balance
  const balanceQuery = useQuery({
    queryKey: ['wallet', 'balance'],
    enabled: !!token,
    queryFn: () => walletApi.getBalance(),
  });

  // Purchase mutation
  const purchaseMutation = useMutation({
    mutationFn: (packageId: string) => paymentsApi.purchasePremium(packageId),
    onSuccess: () => {
      toast.success(t('premium.toast.activated'));
      queryClient.invalidateQueries({ queryKey: ['premium'] });
      queryClient.invalidateQueries({ queryKey: ['wallet'] });
      setSelectedPackageId(null);
    },
    onError: (error: any) => {
      const msg = error?.response?.data?.message ?? t('premium.toast.purchaseFailed');
      toast.error(msg);
    },
  });

  const isPremium = statusQuery.data?.isPremium ?? false;
  const subscription = statusQuery.data?.subscription;
  const packages = packagesQuery.data ?? [];
  const balance = balanceQuery.data?.balance ?? 0;

  // Sort: monthly first, then yearly
  const sortedPackages = useMemo(() => {
    return [...packages].sort((a, b) => a.durationDays - b.durationDays);
  }, [packages]);

  const handlePurchase = (pkg: PremiumPackage) => {
    if (balance < pkg.price) {
      toast.error(t('premium.toast.insufficientBalance', {
        required: formatCurrency(pkg.price, 'VND'),
        current: formatCurrency(balance, 'VND'),
      }));
      return;
    }
    setSelectedPackageId(pkg.id);
  };

  const confirmPurchase = () => {
    if (!selectedPackageId) return;
    purchaseMutation.mutate(selectedPackageId);
  };

  const daysRemaining = subscription
    ? Math.max(0, Math.ceil((new Date(subscription.endDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24)))
    : 0;

  const selectedPackage = packages.find(p => p.id === selectedPackageId);

  // Feature list for display
  const candidateFeatures = [
    { icon: <FileText className="h-5 w-5" />, title: t('premium.candidateFeatures.aiCv'), desc: t('premium.candidateFeatures.aiCvDesc') },
    { icon: <Search className="h-5 w-5" />, title: t('premium.candidateFeatures.matchJobs'), desc: t('premium.candidateFeatures.matchJobsDesc') },
    { icon: <Sparkles className="h-5 w-5" />, title: t('premium.candidateFeatures.unlimitedRec'), desc: t('premium.candidateFeatures.unlimitedRecDesc') },
  ];

  const companyFeatures = [
    { icon: <Eye className="h-5 w-5" />, title: t('premium.companyFeatures.viewCandidate'), desc: t('premium.companyFeatures.viewCandidateDesc') },
    { icon: <Search className="h-5 w-5" />, title: t('premium.companyFeatures.searchCandidate'), desc: t('premium.companyFeatures.searchCandidateDesc') },
    { icon: <Shield className="h-5 w-5" />, title: t('premium.companyFeatures.prioritySupport'), desc: t('premium.companyFeatures.prioritySupportDesc') },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold text-gray-900">
            <Crown className="h-7 w-7 text-amber-500" />
            {t('premium.title')}
          </h1>
          <p className="mt-1 text-sm text-gray-600">
            {t('premium.subtitle')}
          </p>
        </div>
        <div className="flex items-center gap-2 text-sm text-gray-600">
          <Wallet className="h-4 w-4" />
          {t('premium.balance')}: <span className="font-semibold text-gray-900">{formatCurrency(balance, 'VND')}</span>
          <Button variant="outline" size="sm" onClick={() => navigate('/candidate/wallet')}>
            {t('premium.topUp')}
          </Button>
        </div>
      </div>

      {/* Active Subscription Status */}
      {isPremium && subscription && (
        <Card className="border-amber-200 bg-gradient-to-r from-amber-50 to-yellow-50">
          <CardContent className="py-5 px-6">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div className="flex items-center gap-3">
                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-amber-100">
                  <Crown className="h-6 w-6 text-amber-600" />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="font-semibold text-gray-900">{t('premium.activeLabel')}</h3>
                    <Badge variant="success">{t('premium.active')}</Badge>
                  </div>
                  <p className="text-sm text-gray-600">
                    {t('premium.expiresAt', { date: formatDateTime(subscription.endDate), days: daysRemaining })}
                  </p>
                </div>
              </div>
              <div className="flex items-center gap-2">
                <Clock className="h-4 w-4 text-amber-600" />
                <span className="text-sm font-medium text-amber-700">
                  {daysRemaining > 7 ? t('premium.active') : t('premium.expiredIn', { days: daysRemaining })}
                </span>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Pricing Cards */}
      {!isPremium && (
        <div className="grid gap-4 md:grid-cols-2">
          {sortedPackages.length === 0 && !packagesQuery.isLoading && (
            <div className="col-span-2 py-12 text-center">
              <Crown className="mx-auto h-12 w-12 text-gray-300" />
              <p className="mt-3 text-gray-500">{t('premium.noPackages')}</p>
              <p className="text-sm text-gray-400">{t('premium.noPackagesHint')}</p>
            </div>
          )}

          {packagesQuery.isLoading && (
            <div className="col-span-2 flex items-center justify-center py-12">
              <div className="h-8 w-8 animate-spin rounded-full border-2 border-amber-500/30 border-t-amber-500" />
            </div>
          )}

          {sortedPackages.map((pkg) => {
            const isYearly = pkg.durationDays >= 365;
            const isPopular = isYearly;
            const monthlyEquivalent = isYearly ? Math.round(pkg.price / 12) : pkg.price;

            return (
              <Card
                key={pkg.id}
                className={`relative overflow-hidden transition-all hover:shadow-lg ${
                  isPopular
                    ? 'border-2 border-amber-400 shadow-amber-100'
                    : 'border border-gray-200'
                }`}
              >
                {isPopular && (
                  <div className="absolute -right-8 top-4 rotate-45 bg-gradient-to-r from-amber-500 to-orange-500 px-8 py-1 text-xs font-bold text-white shadow-sm">
                    {t('premium.savings')}
                  </div>
                )}

                <CardHeader className="pb-4">
                  <div className="flex items-center gap-2">
                    <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${
                      isPopular ? 'bg-amber-100' : 'bg-gray-100'
                    }`}>
                      <Crown className={`h-5 w-5 ${isPopular ? 'text-amber-600' : 'text-gray-500'}`} />
                    </div>
                    <div>
                      <CardTitle className="text-lg">{pkg.name}</CardTitle>
                      {pkg.description && (
                        <p className="text-xs text-gray-500">{pkg.description}</p>
                      )}
                    </div>
                  </div>
                </CardHeader>

                <CardContent className="space-y-4">
                  {/* Price */}
                  <div>
                    <div className="flex items-baseline gap-1">
                      <span className="text-3xl font-bold text-gray-900">
                        {formatCurrency(pkg.price, 'VND')}
                      </span>
                      <span className="text-sm text-gray-500">
                        {isYearly ? t('premium.perYear') : t('premium.perMonth')}
                      </span>
                    </div>
                    {isYearly && (
                      <p className="mt-1 text-xs text-amber-600 font-medium">
                        {t('premium.perMonthEquiv', { amount: formatCurrency(monthlyEquivalent, 'VND') })}
                      </p>
                    )}
                  </div>

                  {/* Duration */}
                  <p className="text-sm text-gray-600">
                    {t('premium.duration')}: <span className="font-medium">{t('premium.durationDays', { days: pkg.durationDays })}</span>
                  </p>

                  {/* Features */}
                  {pkg.features.length > 0 && (
                    <ul className="space-y-2">
                      {pkg.features.map((feature, i) => (
                        <li key={i} className="flex items-start gap-2 text-sm text-gray-700">
                          <Check className="mt-0.5 h-4 w-4 flex-shrink-0 text-green-500" />
                          <span>{feature}</span>
                        </li>
                      ))}
                    </ul>
                  )}

                  {/* Purchase button */}
                  <Button
                    className={`w-full ${
                      isPopular
                        ? 'bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white'
                        : 'bg-[#00b14f] hover:bg-[#00953f] text-white'
                    }`}
                    onClick={() => handlePurchase(pkg)}
                    disabled={purchaseMutation.isPending}
                  >
                    {balance >= pkg.price ? (
                      <span className="flex items-center gap-2">
                        <Crown className="h-4 w-4" />
                        {t('premium.buyNow')}
                        <ArrowRight className="h-4 w-4" />
                      </span>
                    ) : (
                      <span className="flex items-center gap-2">
                        <Wallet className="h-4 w-4" />
                        {t('premium.needTopUp')}
                      </span>
                    )}
                  </Button>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}

      {/* Confirm Modal */}
      {selectedPackageId && selectedPackage && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <Card className="w-full max-w-md animate-in fade-in zoom-in">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Crown className="h-5 w-5 text-amber-500" />
                {t('premium.confirm.title')}
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="rounded-lg bg-gray-50 p-4 space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">{t('premium.confirm.package')}:</span>
                  <span className="font-medium text-gray-900">{selectedPackage.name}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">{t('premium.confirm.price')}:</span>
                  <span className="font-bold text-gray-900">{formatCurrency(selectedPackage.price, 'VND')}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">{t('premium.confirm.duration')}:</span>
                  <span className="font-medium text-gray-900">{t('premium.durationDays', { days: selectedPackage.durationDays })}</span>
                </div>
                <hr className="my-2" />
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">{t('premium.confirm.currentBalance')}:</span>
                  <span className="font-medium text-gray-900">{formatCurrency(balance, 'VND')}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">{t('premium.confirm.afterPurchase')}:</span>
                  <span className="font-bold text-green-600">
                    {formatCurrency(balance - selectedPackage.price, 'VND')}
                  </span>
                </div>
              </div>

              <p className="text-xs text-gray-500 text-center">
                {t('premium.confirm.deductNote')}
              </p>

              <div className="flex gap-3">
                <Button
                  variant="outline"
                  className="flex-1"
                  onClick={() => setSelectedPackageId(null)}
                  disabled={purchaseMutation.isPending}
                >
                  {t('premium.confirm.cancel')}
                </Button>
                <Button
                  className="flex-1 bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white"
                  onClick={confirmPurchase}
                  isLoading={purchaseMutation.isPending}
                >
                  {purchaseMutation.isPending ? t('premium.confirm.processing') : t('premium.confirm.confirm')}
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Feature Showcase */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Candidate Features */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="flex items-center gap-2 text-base">
              <FileText className="h-5 w-5 text-blue-500" />
              {t('premium.candidateFeatures.title')}
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {candidateFeatures.map((feature, i) => (
              <div key={i} className="flex items-start gap-3">
                <div className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-lg bg-blue-50 text-blue-500">
                  {feature.icon}
                </div>
                <div>
                  <h4 className="text-sm font-semibold text-gray-900">{feature.title}</h4>
                  <p className="text-xs text-gray-500">{feature.desc}</p>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        {/* Company Features */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="flex items-center gap-2 text-base">
              <Search className="h-5 w-5 text-purple-500" />
              {t('premium.companyFeatures.title')}
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {companyFeatures.map((feature, i) => (
              <div key={i} className="flex items-start gap-3">
                <div className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-lg bg-purple-50 text-purple-500">
                  {feature.icon}
                </div>
                <div>
                  <h4 className="text-sm font-semibold text-gray-900">{feature.title}</h4>
                  <p className="text-xs text-gray-500">{feature.desc}</p>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>

      {/* Free vs Premium comparison */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">{t('premium.comparison.title')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="py-3 pr-4 text-left font-medium text-gray-500">{t('premium.comparison.feature')}</th>
                  <th className="px-4 py-3 text-center font-medium text-gray-500">{t('premium.comparison.free')}</th>
                  <th className="px-4 py-3 text-center font-medium text-amber-600">
                    <span className="flex items-center justify-center gap-1">
                      <Crown className="h-4 w-4" /> {t('premium.comparison.premium')}
                    </span>
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                <tr>
                  <td className="py-3 pr-4 text-gray-700">{t('premium.comparison.jobRec')}</td>
                  <td className="px-4 py-3 text-center text-gray-500">{t('premium.comparison.jobRecFree')}</td>
                  <td className="px-4 py-3 text-center font-medium text-green-600">{t('premium.comparison.jobRecPremium')}</td>
                </tr>
                <tr>
                  <td className="py-3 pr-4 text-gray-700">{t('premium.comparison.aiCvGen')}</td>
                  <td className="px-4 py-3 text-center text-red-400">✗</td>
                  <td className="px-4 py-3 text-center text-green-600">✓</td>
                </tr>
                <tr>
                  <td className="py-3 pr-4 text-gray-700">{t('premium.comparison.matchScore')}</td>
                  <td className="px-4 py-3 text-center text-red-400">✗</td>
                  <td className="px-4 py-3 text-center text-green-600">✓</td>
                </tr>
                <tr>
                  <td className="py-3 pr-4 text-gray-700">{t('premium.comparison.searchCandidate')}</td>
                  <td className="px-4 py-3 text-center text-red-400">✗</td>
                  <td className="px-4 py-3 text-center text-green-600">✓</td>
                </tr>
                <tr>
                  <td className="py-3 pr-4 text-gray-700">{t('premium.comparison.viewCandidate')}</td>
                  <td className="px-4 py-3 text-center text-red-400">✗</td>
                  <td className="px-4 py-3 text-center text-green-600">✓</td>
                </tr>
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
