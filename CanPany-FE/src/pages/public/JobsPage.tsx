import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Search, Filter, MapPin, Briefcase, X, SlidersHorizontal } from 'lucide-react';
import { Button, Input, Badge } from '../../components/ui';
import { JobCard } from '../../components/features/jobs';
import { jobsApi } from '../../api';
import type { Job, JobLevel, BudgetType } from '../../types';

const LEVELS: JobLevel[] = ['Junior', 'Mid', 'Senior', 'Expert'];

export function JobsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [keyword, setKeyword] = useState(searchParams.get('keyword') || '');
  const [location, setLocation] = useState(searchParams.get('location') || '');
  const [showFilters, setShowFilters] = useState(false);

  // Filter states - changed to single value instead of array
  const [selectedLevel, setSelectedLevel] = useState<JobLevel | ''>(() => {
    return (searchParams.get('level') as JobLevel) || '';
  });
  const [selectedBudgetType, setSelectedBudgetType] = useState<BudgetType | ''>(() => {
    return (searchParams.get('budgetType') as BudgetType) || '';
  });
  const [isRemote, setIsRemote] = useState(() => searchParams.get('isRemote') === 'true');

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

  const { data: jobs = [], isLoading } = useQuery({
    queryKey: ['jobs', searchParams.toString()],
    queryFn: () => jobsApi.search({
      keyword: searchParams.get('keyword') || undefined,
      categoryId: searchParams.get('categoryId') || undefined,
      location: searchParams.get('location') || undefined,
      level: (searchParams.get('level') as JobLevel) || undefined,
      budgetType: (searchParams.get('budgetType') as BudgetType) || undefined,
      isRemote: searchParams.get('isRemote') === 'true' || undefined,
    }),
  });

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
    <div className="min-h-screen bg-gray-50">
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
            <div className="flex flex-col gap-3 rounded-xl bg-white p-2 shadow-xl sm:flex-row">
              <div className="relative flex-1">
                <Search className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  value={keyword}
                  onChange={(e) => setKeyword(e.target.value)}
                  placeholder="Vị trí tuyển dụng, tên công ty..."
                  className="w-full rounded-lg border-0 bg-gray-50 py-3.5 pl-12 pr-4 text-gray-900 placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                />
              </div>
              <div className="relative flex-1">
                <MapPin className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  value={location}
                  onChange={(e) => setLocation(e.target.value)}
                  placeholder="Địa điểm..."
                  className="w-full rounded-lg border-0 bg-gray-50 py-3.5 pl-12 pr-4 text-gray-900 placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                />
              </div>
              <Button type="submit" size="lg" className="sm:px-8">
                <Search className="h-4 w-4" />
                Tìm kiếm
              </Button>
            </div>
          </form>
        </div>
      </div>

      {/* Content */}
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-6 lg:flex-row">
          {/* Filters Sidebar */}
          <div className="hidden lg:block lg:w-72">
            <div className="sticky top-24 space-y-6">
              <div className="rounded-xl border border-gray-100 bg-white p-5">
                <h3 className="flex items-center gap-2 font-semibold text-gray-900">
                  <SlidersHorizontal className="h-5 w-5 text-[#00b14f]" />
                  Bộ lọc tìm kiếm
                </h3>
                <div className="mt-4 space-y-4">
                  {/* Level Filter */}
                  <div>
                    <label className="text-sm font-medium text-gray-700">Cấp bậc</label>
                    <div className="mt-2 space-y-2">
                      {LEVELS.map((level) => (
                        <label key={level} className="flex cursor-pointer items-center gap-2">
                          <input
                            type="checkbox"
                            checked={selectedLevel === level}
                            onChange={() => handleLevelChange(level)}
                            className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f]"
                          />
                          <span className="text-sm text-gray-600">{level}</span>
                        </label>
                      ))}
                    </div>
                  </div>

                  {/* Budget Type Filter */}
                  <div>
                    <label className="text-sm font-medium text-gray-700">Hình thức lương</label>
                    <div className="mt-2 space-y-2">
                      <label className="flex cursor-pointer items-center gap-2">
                        <input
                          type="checkbox"
                          checked={selectedBudgetType === 'Fixed'}
                          onChange={() => handleBudgetTypeChange('Fixed')}
                          className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f]"
                        />
                        <span className="text-sm text-gray-600">Cố định</span>
                      </label>
                      <label className="flex cursor-pointer items-center gap-2">
                        <input
                          type="checkbox"
                          checked={selectedBudgetType === 'Hourly'}
                          onChange={() => handleBudgetTypeChange('Hourly')}
                          className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f]"
                        />
                        <span className="text-sm text-gray-600">Theo giờ</span>
                      </label>
                    </div>
                  </div>

                  {/* Remote Filter */}
                  <div>
                    <label className="text-sm font-medium text-gray-700">Hình thức làm việc</label>
                    <div className="mt-2 space-y-2">
                      <label className="flex cursor-pointer items-center gap-2">
                        <input
                          type="checkbox"
                          checked={isRemote}
                          onChange={(e) => setIsRemote(e.target.checked)}
                          className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f]"
                        />
                        <span className="text-sm text-gray-600">Remote</span>
                      </label>
                    </div>
                  </div>

                  {/* Apply Filters Button */}
                  <Button
                    type="button"
                    className="w-full mt-4"
                    onClick={applyFilters}
                  >
                    <Filter className="h-4 w-4 mr-2" />
                    Áp dụng lọc
                  </Button>

                  {/* Clear Filters Button */}
                  {hasActiveFilters && (
                    <Button
                      type="button"
                      variant="outline"
                      className="w-full mt-2"
                      onClick={clearFilters}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Xóa bộ lọc
                    </Button>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Job List */}
          <div className="flex-1">
            {/* Active Filters */}
            {(searchParams.get('keyword') || searchParams.get('location') || searchParams.get('level') || searchParams.get('budgetType') || searchParams.get('isRemote')) && (
              <div className="mb-4 flex flex-wrap items-center gap-2">
                <span className="text-sm text-gray-500">Bộ lọc đang áp dụng:</span>
                {searchParams.get('keyword') && (
                  <Badge variant="default" className="gap-1">
                    Từ khóa: {searchParams.get('keyword')}
                    <button onClick={() => { setKeyword(''); applyFilters(); }} className="ml-1">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                )}
                {searchParams.get('location') && (
                  <Badge variant="default" className="gap-1">
                    Địa điểm: {searchParams.get('location')}
                    <button onClick={() => { setLocation(''); applyFilters(); }} className="ml-1">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                )}
                {searchParams.get('level') && (
                  <Badge variant="default" className="gap-1">
                    Cấp bậc: {searchParams.get('level')}
                    <button onClick={() => { setSelectedLevel(''); applyFilters(); }} className="ml-1">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                )}
                {searchParams.get('budgetType') && (
                  <Badge variant="default" className="gap-1">
                    Hình thức: {searchParams.get('budgetType') === 'Fixed' ? 'Cố định' : 'Theo giờ'}
                    <button onClick={() => { setSelectedBudgetType(''); applyFilters(); }} className="ml-1">
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                )}
                {searchParams.get('isRemote') === 'true' && (
                  <Badge variant="default" className="gap-1">
                    Remote
                    <button onClick={() => { setIsRemote(false); applyFilters(); }} className="ml-1">
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
              <p className="text-sm text-gray-600">
                {isLoading ? 'Đang tải...' : (
                  <>Tìm thấy <span className="font-semibold text-[#00b14f]">{jobs.length}</span> việc làm</>
                )}
              </p>
              <select className="rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm focus:border-[#00b14f] focus:outline-none focus:ring-1 focus:ring-[#00b14f]/20">
                <option>Mới nhất</option>
                <option>Lương cao nhất</option>
                <option>Phù hợp nhất</option>
              </select>
            </div>

            {/* Job List */}
            {isLoading ? (
              <div className="space-y-4">
                {[1, 2, 3].map((i) => (
                  <div key={i} className="h-44 animate-pulse rounded-xl bg-gray-200" />
                ))}
              </div>
            ) : jobs.length === 0 ? (
              <div className="rounded-xl border border-gray-100 bg-white py-16 text-center">
                <div className="mx-auto h-20 w-20 rounded-full bg-gray-100 p-5">
                  <Briefcase className="h-10 w-10 text-gray-400" />
                </div>
                <h3 className="mt-4 text-lg font-semibold text-gray-900">Không tìm thấy việc làm</h3>
                <p className="mt-2 text-gray-500">Thử thay đổi từ khóa hoặc điều chỉnh bộ lọc</p>
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
          </div>
        </div>
      </div>
    </div>
  );
}
