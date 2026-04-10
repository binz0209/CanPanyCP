import { useQuery } from '@tanstack/react-query';
import { applicationsApi } from '../../api/applications.api';
import { companiesApi } from '../../api/companies.api';
import { jobsApi } from '../../api/jobs.api';
import { applicationKeys } from '../../lib/queryKeys';
import type { Application } from '../../types/application.types';

export async function fetchMyApplicationsHydrated(): Promise<Application[]> {
    const data = await applicationsApi.getMyApplications();

    const uniqueJobIds = Array.from(new Set(data.map((app) => app.jobId).filter(Boolean)));
    const jobsById = new Map<string, NonNullable<Application['job']>>();

    if (uniqueJobIds.length > 0) {
        const jobResponses = await Promise.allSettled(
            uniqueJobIds.map((jobId) => jobsApi.getById(jobId))
        );

        jobResponses.forEach((result, index) => {
            if (result.status === 'fulfilled' && result.value?.job) {
                jobsById.set(uniqueJobIds[index], result.value.job);
            }
        });

        const uniqueCompanyIds = Array.from(
            new Set(
                Array.from(jobsById.values())
                    .map((job) => job.companyId)
                    .filter(Boolean)
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

    return data.map((app) => ({
        ...app,
        job: app.job ?? jobsById.get(app.jobId),
    }));
}

interface UseCandidateApplicationsOptions {
    enabled?: boolean;
}

export function useCandidateApplications(options?: UseCandidateApplicationsOptions) {
    const { enabled = true } = options || {};

    return useQuery({
        queryKey: applicationKeys.mine(),
        queryFn: fetchMyApplicationsHydrated,
        staleTime: 30_000,
        enabled,
    });
}
