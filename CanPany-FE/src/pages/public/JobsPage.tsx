import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Search, Filter, MapPin, Briefcase, X, SlidersHorizontal } from 'lucide-react';
import { Button, Input, Badge } from '../../components/ui';
import { JobCard } from '../../components/features/jobs';
import { jobsApi } from '../../api';
import type { Job } from '../../types';

export function JobsPage() {
    const [searchParams, setSearchParams] = useSearchParams();
    const [keyword, setKeyword] = useState(searchParams.get('keyword') || '');
    const [showFilters, setShowFilters] = useState(false);

    const { data: jobs = [], isLoading } = useQuery({
        queryKey: ['jobs', searchParams.toString()],
        queryFn: () => jobsApi.search({
            keyword: searchParams.get('keyword') || undefined,
            categoryId: searchParams.get('categoryId') || undefined,
        }),
    });

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        if (keyword.trim()) {
            setSearchParams({ keyword: keyword.trim() });
        } else {
            setSearchParams({});
        }
    };

    const clearFilters = () => {
        setKeyword('');
        setSearchParams({});
    };

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
                            <div className="relative hidden flex-1 sm:block">
                                <MapPin className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                                <select className="w-full appearance-none rounded-lg border-0 bg-gray-50 py-3.5 pl-12 pr-4 text-gray-900 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20">
                                    <option>Tất cả địa điểm</option>
                                    <option>Hà Nội</option>
                                    <option>TP. Hồ Chí Minh</option>
                                    <option>Đà Nẵng</option>
                                </select>
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
                                    <div>
                                        <label className="text-sm font-medium text-gray-700">Cấp bậc</label>
                                        <div className="mt-2 space-y-2">
                                            {['Junior', 'Mid', 'Senior', 'Expert'].map((level) => (
                                                <label key={level} className="flex items-center gap-2">
                                                    <input type="checkbox" className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f]" />
                                                    <span className="text-sm text-gray-600">{level}</span>
                                                </label>
                                            ))}
                                        </div>
                                    </div>

                                    <div>
                                        <label className="text-sm font-medium text-gray-700">Hình thức</label>
                                        <div className="mt-2 space-y-2">
                                            {['Toàn thời gian', 'Bán thời gian', 'Remote', 'Freelance'].map((type) => (
                                                <label key={type} className="flex items-center gap-2">
                                                    <input type="checkbox" className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f]" />
                                                    <span className="text-sm text-gray-600">{type}</span>
                                                </label>
                                            ))}
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Job List */}
                    <div className="flex-1">
                        {/* Active Filters */}
                        {searchParams.get('keyword') && (
                            <div className="mb-4 flex items-center gap-2">
                                <span className="text-sm text-gray-500">Kết quả cho:</span>
                                <Badge variant="default" className="gap-1">
                                    {searchParams.get('keyword')}
                                    <button onClick={clearFilters} className="ml-1">
                                        <X className="h-3 w-3" />
                                    </button>
                                </Badge>
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
