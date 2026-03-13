import { useState, useEffect, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Search, Filter, MapPin, Briefcase, X, SlidersHorizontal, Loader2 } from 'lucide-react';
import { Button, Badge } from '../../components/ui';
import { JobCard } from '../../components/features/jobs';
import { jobsApi, companiesApi } from '../../api';
import type { Job, JobLevel, BudgetType, Company } from '../../types';

const LEVELS: JobLevel[] = ['Junior', 'Mid', 'Senior', 'Expert'];

export function JobsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [keyword, setKeyword] = useState(searchParams.get('keyword') || '');
  const [location, setLocation] = useState(searchParams.get('location') || '');
  const [showFilters, setShowFilters] = useState(false);

  // Filter states
  const [selectedLevel, setSelectedLevel] = useState<JobLevel | ''>(() => {
    return (searchParams.get('level') as JobLevel) || '';
  });
  const [selectedBudgetType, setSelectedBudgetType] = useState<BudgetType | ''>(() => {
    return (searchParams.get('budgetType') as BudgetType) || '';
  });
  const [isRemote, setIsRemote] = useState(() => searchParams.get('isRemote') === 'true');

  // Debounced keyword and location
  const [debouncedKeyword, setDebouncedKeyword] = useState(keyword);
  const [debouncedLocation, setDebouncedLocation] = useState(location);

  // Debounce effect
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedKeyword(keyword);
    }, 300);
    return () => clearTimeout(timer);
  }, [keyword]);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedLocation(location);
    }, 300);
    return () => clearTimeout(timer);
  }, [location]);

  // Fetch all jobs - backend only supports keyword search
  const { data: allJobs = [], isLoading } = useQuery<Job[]>({
    queryKey: ['jobs', debouncedKeyword],
    queryFn: () => jobsApi.search({
      keyword: debouncedKeyword || undefined,
    }),
  });

  // Extract unique company IDs from jobs
  const companyIds = useMemo(() => {
    const ids = new Set<string>();
    allJobs.forEach(job => {
      if (job.companyId) {
        ids.add(job.companyId);
      }
    });
    return Array.from(ids);
  }, [allJobs]);

  // Fetch companies for the jobs
  const { data: companiesData, isLoading: isLoadingCompanies } = useQuery<Company[]>({
    queryKey: ['companies-for-jobs', companyIds],
    queryFn: async () => {
      if (companyIds.length === 0) return [];
      // Fetch companies sequentially
      const companies: Company[] = [];
      for (const id of companyIds) {
        try {
          const company = await companiesApi.getById(id);
          companies.push(company);
        } catch (e) {
          // Skip failed company fetches
        }
      }
      return companies;
    },
    enabled: companyIds.length > 0,
  });

  // Create company map for quick lookup
  const companyMap = useMemo(() => {
    const map = new Map<string, Company>();
    companiesData?.forEach(company => {
      map.set(company.id, company);
    });
    return map;
  }, [companiesData]);

  // Enrich jobs with company data
  const jobsWithCompany = useMemo(() => {
    return allJobs.map(job => ({
      ...job,
      company: job.companyId ? companyMap.get(job.companyId) : undefined,
    }));
  }, [allJobs, companyMap]);

  // Client-side filtering
  const jobs = useMemo(() => {
    return jobsWithCompany.filter(job => {
      // Filter by location
      if (debouncedLocation && job.location) {
        const searchLoc = debouncedLocation.toLowerCase();
        if (!job.location.toLowerCase().includes(searchLoc)) {
          return false;
        }
      }
      
      // Filter by level
      if (selectedLevel && job.level) {
        if (job.level !== selectedLevel) {
          return false;
        }
      }
      
      // Filter by budget type
      if (selectedBudgetType && job.budgetType) {
        if (job.budgetType !== selectedBudgetType) {
          return false;
        }
      }
      
      // Filter by remote
      if (isRemote && !job.isRemote) {
        return false;
      }
      
      return true;
    });
  }, [jobsWithCompany, debouncedLocation, selectedLevel, selectedBudgetType, isRemote]);

  const isLoadingAny = isLoading || isLoadingCompanies;

  // Build URL search params
  const buildSearchParams = (): Record<string, string> => {
    const params: Record<string, string> = {};
    if (keyword.trim()) params.keyword = keyword.trim();
    if (location.trim()) params.location = location.trim();
    if (selectedLevel) params.level = selectedLevel;
    if (selectedBudgetType) params.budgetType = selectedBudgetType;
    if (isRemote) params.isRemote = 'true';
    const categoryId = searchParams.get('categoryId');
    if (categoryId) params.categoryId = categoryId;
    return params;
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const params = buildSearchParams();
    const urlParams = new URLSearchParams(params);
    setSearchParams(urlParams);
  };

  const handleLevelChange = (level: JobLevel) => {
    setSelectedLevel(prev => prev === level ? '' : level);
  };

  const handleBudgetTypeChange = (type: BudgetType) => {
    setSelectedBudgetType(prev => prev === type ? '' : type);
  };

  const applyFilters = () => {
    const params = buildSearchParams();
    const urlParams = new URLSearchParams(params);
    setSearchParams(urlParams);
  };

  const clearFilters = () => {
    setKeyword('');
    setLocation('');
    setSelectedLevel('');
    setSelectedBudgetType('');
    setIsRemote(false);
    setSearchParams(new URLSearchParams());
  };

  // Check if any filter is active
  const hasActiveFilters = keyword || location || selectedLevel || selectedBudgetType || isRemote || searchParams.get('categoryId');

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-slate-950">
      {/* Search Header - TopCV Style */}
      <div className="bg-gradient-to-r from-[#00b14f] to-[#008f3c]">
        <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-white sm:text-3xl">
              Tìm việc làm nhanh 24h, việc làm mới nhất trên toàn quốc
            </h1>
            <p className="mt-2 text-white/80">
              Tiếp cận <span className="font-semibold">50,000+</span> tin tuyển dụng việc làm mỗi ngày
            </p>
          </div>
          <form onSubmit={handleSearch} className="mt-6">
            <div className="flex flex-col gap-3 rounded-xl bg-white p-2 shadow-xl dark:bg-slate-900 sm:flex-row">
              <div className="relative flex-1">
                <Search className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" aria-hidden="true" />
                <input
                  type="text"
                  value={keyword}
                  onChange={(e) => setKeyword(e.target.value)}
                  placeholder="Vị trí tuyển dụng, tên công ty..."
                  className="w-full rounded-lg border-0 bg-gray-50 py-3.5 pl-12 pr-4 text-gray-900 placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20 dark:bg-slate-800 dark:text-slate-100 dark:placeholder:text-slate-400"
                  aria-label="Tìm kiếm việc làm"
                />
              </div>
              <div className="relative flex-1">
                <MapPin className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" aria-hidden="true" />
                <input
                  type="text"
                  value={location}
                  onChange={(e) => setLocation(e.target.value)}
                  placeholder="Địa điểm..."
                  className="w-full rounded-lg border-0 bg-gray-50 py-3.5 pl-12 pr-4 text-gray-900 placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20 dark:bg-slate-800 dark:text-slate-100 dark:placeholder:text-slate-400"
                  aria-label="Địa điểm"
                />
              </div>
              <Button type="submit" size="lg" className="sm:px-8">
                <Search className="h-4 w-4" aria-hidden="true" />
                Tìm kiếm
              </Button>
            </div>
          </form>
        </div>
      </div>

      {/* Content */}
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-6 lg:flex-row">
          {/* Filters Sidebar - Desktop */}
          <div className="hidden lg:block lg:w-72">
            <div className="sticky top-24 space-y-6">
              <div className="rounded-xl border border-gray-100 bg-white p-5 dark:border-slate-800 dark:bg-slate-900">
                <h3 className="flex items-center gap-2 font-semibold text-gray-900 dark:text-slate-100">
                  <SlidersHorizontal className="h-5 w-5 text-[#00b14f]" aria-hidden="true" />
                  Bộ lọc tìm kiếm
                </h3>
                <div className="mt-4 space-y-4">
                  {/* Level Filter */}
                  <div>
                    <label className="text-sm font-medium text-gray-700 dark:text-slate-200">Cấp bậc</label>
                    <div className="mt-2 space-y-2">
                      {LEVELS.map((level) => (
                        <label key={level} className="flex cursor-pointer items-center gap-2">
                          <input
                            type="checkbox"
                            checked={selectedLevel === level}
                            onChange={() => handleLevelChange(level)}
                            className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f] dark:border-slate-600"
                          />
                          <span className="text-sm text-gray-600 dark:text-slate-300">{level}</span>
                        </label>
                      ))}
                    </div>
                  </div>

                  {/* Budget Type Filter */}
                  <div>
                    <label className="text-sm font-medium text-gray-700 dark:text-slate-200">Hình thức lương</label>
                    <div className="mt-2 space-y-2">
                      <label className="flex cursor-pointer items-center gap-2">
                        <input
                          type="checkbox"
                          checked={selectedBudgetType === 'Fixed'}
                          onChange={() => handleBudgetTypeChange('Fixed')}
                          className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f] dark:border-slate-600"
                        />
                        <span className="text-sm text-gray-600 dark:text-slate-300">Cố định</span>
                      </label>
                      <label className="flex cursor-pointer items-center gap-2">
                        <input
                          type="checkbox"
                          checked={selectedBudgetType === 'Hourly'}
                          onChange={() => handleBudgetTypeChange('Hourly')}
                          className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f] dark:border-slate-600"
                        />
                        <span className="text-sm text-gray-600 dark:text-slate-300">Theo giờ</span>
                      </label>
                    </div>
                  </div>

                  {/* Remote Filter */}
                  <div>
                    <label className="text-sm font-medium text-gray-700 dark:text-slate-200">Hình thức làm việc</label>
                    <div className="mt-2 space-y-2">
                      <label className="flex cursor-pointer items-center gap-2">
                        <input
                          type="checkbox"
                          checked={isRemote}
                          onChange={(e) => setIsRemote(e.target.checked)}
                          className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f] dark:border-slate-600"
                        />
                        <span className="text-sm text-gray-600 dark:text-slate-300">Remote</span>
                      </label>
                    </div>
                  </div>

                  {/* Apply Filters Button */}
                  <Button
                    type="button"
                    className="mt-4 w-full"
                    onClick={applyFilters}
                  >
                    <Filter className="mr-2 h-4 w-4" aria-hidden="true" />
                    Áp dụng lọc
                  </Button>

                  {/* Clear Filters Button */}
                  {hasActiveFilters && (
                    <Button
                      type="button"
                      variant="outline"
                      className="mt-2 w-full"
                      onClick={clearFilters}
                    >
                      <X className="mr-2 h-4 w-4" aria-hidden="true" />
                      Xóa bộ lọc
                    </Button>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Mobile Filter Button */}
          <div className="lg:hidden">
            <Button
              variant="outline"
              className="w-full"
              onClick={() => setShowFilters(true)}
              aria-label="Mở bộ lọc"
            >
              <Filter className="mr-2 h-4 w-4" aria-hidden="true" />
              Bộ lọc
              {hasActiveFilters && (
                <Badge className="ml-2 bg-[#00b14f] text-white">
                  {(keyword ? 1 : 0) + (location ? 1 : 0) + (selectedLevel ? 1 : 0) + (selectedBudgetType ? 1 : 0) + (isRemote ? 1 : 0)}
                </Badge>
              )}
            </Button>
          </div>

          {/* Mobile Filter Drawer */}
          {showFilters && (
            <div className="fixed inset-0 z-50 lg:hidden">
              <div className="absolute inset-0 bg-black/50" onClick={() => setShowFilters(false)} aria-hidden="true" />
              <div className="absolute bottom-0 left-0 right-0 max-h-[80vh] overflow-y-auto rounded-t-2xl bg-white p-6 dark:bg-slate-900">
                <div className="mb-4 flex items-center justify-between">
                  <h3 className="text-lg font-semibold dark:text-slate-100">Bộ lọc tìm kiếm</h3>
                  <button
                    onClick={() => setShowFilters(false)}
                    className="rounded-full p-2 hover:bg-gray-100 dark:hover:bg-slate-800"
                    aria-label="Đóng bộ lọc"
                  >
                    <X className="h-5 w-5 dark:text-slate-200" />
                  </button>
                </div>

                {/* Level Filter */}
                <div className="mb-4">
                  <label className="text-sm font-medium text-gray-700 dark:text-slate-200">Cấp bậc</label>
                  <div className="mt-2 flex flex-wrap gap-2">
                    {LEVELS.map((level) => (
                      <button
                        key={level}
                        onClick={() => handleLevelChange(level)}
                        className={`rounded-full px-4 py-2 text-sm font-medium transition-colors ${
                          selectedLevel === level
                            ? 'bg-[#00b14f] text-white'
                            : 'bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-slate-800 dark:text-slate-200 dark:hover:bg-slate-700'
                        }`}
                      >
                        {level}
                      </button>
                    ))}
                  </div>
                </div>

                {/* Budget Type Filter */}
                <div className="mb-4">
                  <label className="text-sm font-medium text-gray-700 dark:text-slate-200">Hình thức lương</label>
                  <div className="mt-2 flex flex-wrap gap-2">
                    <button
                      onClick={() => handleBudgetTypeChange('Fixed')}
                      className={`rounded-full px-4 py-2 text-sm font-medium transition-colors ${
                        selectedBudgetType === 'Fixed'
                          ? 'bg-[#00b14f] text-white'
                          : 'bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-slate-800 dark:text-slate-200 dark:hover:bg-slate-700'
                      }`}
                    >
                      Cố định
                    </button>
                    <button
                      onClick={() => handleBudgetTypeChange('Hourly')}
                      className={`rounded-full px-4 py-2 text-sm font-medium transition-colors ${
                        selectedBudgetType === 'Hourly'
                          ? 'bg-[#00b14f] text-white'
                          : 'bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-slate-800 dark:text-slate-200 dark:hover:bg-slate-700'
                      }`}
                    >
                      Theo giờ
                    </button>
                  </div>
                </div>

                {/* Remote Filter */}
                <div className="mb-6">
                  <label className="flex cursor-pointer items-center gap-2">
                    <input
                      type="checkbox"
                      checked={isRemote}
                      onChange={(e) => setIsRemote(e.target.checked)}
                      className="h-5 w-5 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f] dark:border-slate-600"
                    />
                    <span className="font-medium text-gray-700 dark:text-slate-200">Remote</span>
                  </label>
                </div>

                <div className="flex gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    className="flex-1"
                    onClick={clearFilters}
                  >
                    Xóa bộ lọc
                  </Button>
                  <Button
                    type="button"
                    className="flex-1"
                    onClick={() => {
                      applyFilters();
                      setShowFilters(false);
                    }}
                  >
                    Áp dụng
                  </Button>
                </div>
              </div>
            </div>
          )}

          {/* Job List */}
          <div className="flex-1">
            {/* Active Filters */}
            {hasActiveFilters && (
              <div className="mb-4 flex flex-wrap items-center gap-2">
                <span className="text-sm text-gray-500 dark:text-slate-400">Bộ lọc đang áp dụng:</span>
                {searchParams.get('keyword') && (
                  <Badge variant="default" className="gap-1">
                    Từ khóa: {searchParams.get('keyword')}
                    <button onClick={() => { setKeyword(''); applyFilters(); }} className="ml-1" aria-label="Xóa từ khóa">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                )}
                {searchParams.get('location') && (
                  <Badge variant="default" className="gap-1">
                    Địa điểm: {searchParams.get('location')}
                    <button onClick={() => { setLocation(''); applyFilters(); }} className="ml-1" aria-label="Xóa địa điểm">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                )}
                {searchParams.get('level') && (
                  <Badge variant="default" className="gap-1">
                    Cấp bậc: {searchParams.get('level')}
                    <button onClick={() => { setSelectedLevel(''); applyFilters(); }} className="ml-1" aria-label="Xóa cấp bậc">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                )}
                {searchParams.get('budgetType') && (
                  <Badge variant="default" className="gap-1">
                    Hình thức: {searchParams.get('budgetType') === 'Fixed' ? 'Cố định' : 'Theo giờ'}
                    <button onClick={() => { setSelectedBudgetType(''); applyFilters(); }} className="ml-1" aria-label="Xóa hình thức lương">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                )}
                {searchParams.get('isRemote') === 'true' && (
                  <Badge variant="default" className="gap-1">
                    Remote
                    <button onClick={() => { setIsRemote(false); applyFilters(); }} className="ml-1" aria-label="Xóa Remote">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                )}
                <button
                  onClick={clearFilters}
                  className="text-sm text-[#00b14f] hover:underline"
                >
                  Xóa tất cả
                </button>
              </div>
            )}

            <div className="mb-4 flex items-center justify-between">
              <p className="text-sm text-gray-600 dark:text-slate-300">
                {isLoadingAny ? (
                  <span className="flex items-center gap-2">
                    <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
                    Đang tải...
                  </span>
                ) : (
                  <>Tìm thấy <span className="font-semibold text-[#00b14f]">{jobs.length}</span> việc làm</>
                )}
              </p>
            </div>

            {/* Job List */}
            {isLoadingAny ? (
              <div className="space-y-4">
                {[1, 2, 3].map((i) => (
                  <div key={i} className="h-44 animate-pulse rounded-xl bg-gray-200 dark:bg-slate-800" />
                ))}
              </div>
            ) : jobs.length === 0 ? (
              <div className="rounded-xl border border-gray-100 bg-white py-16 text-center dark:border-slate-800 dark:bg-slate-900">
                <div className="mx-auto h-20 w-20 rounded-full bg-gray-100 p-5 dark:bg-slate-800">
                  <Briefcase className="h-10 w-10 text-gray-400 dark:text-slate-500" aria-hidden="true" />
                </div>
                <h3 className="mt-4 text-lg font-semibold text-gray-900 dark:text-slate-100">Không tìm thấy việc làm</h3>
                <p className="mt-2 text-gray-500 dark:text-slate-400">Thử thay đổi từ khóa hoặc điều chỉnh bộ lọc</p>
                <Button variant="outline" className="mt-4" onClick={clearFilters}>
                  Xóa bộ lọc
                </Button>
              </div>
            ) : (
              <div className="space-y-4">
                {jobs.map((job: Job) => (
                  <JobCard key={job.id} job={job} />
                ))}
              </div>
            )}

            {/* Results count for screen readers */}
            <p className="sr-only" aria-live="polite">
              {jobs.length > 0 ? `Tìm thấy ${jobs.length} việc làm` : 'Không tìm thấy việc làm nào'}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
