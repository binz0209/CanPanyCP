import { Link } from 'react-router-dom';
import { MapPin, CheckCircle, ExternalLink, Building2 } from 'lucide-react';
import { Card, Badge, Button } from '@/components/ui';
import type { Company } from '@/types';
import { cn } from '@/utils';

interface CompanyCardProps {
    company: Company;
}

export function CompanyCard({ company }: CompanyCardProps) {
    return (
        <Card className="group p-6 transition hover:shadow-lg">
            <div className="flex items-start gap-4">
                {/* Company Logo */}
                {company.logoUrl ? (
                    <img
                        src={company.logoUrl}
                        alt={company.name}
                        className="h-16 w-16 rounded-xl object-cover"
                    />
                ) : (
                    <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-blue-100 to-purple-100">
                        <Building2 className="h-8 w-8 text-blue-600" />
                    </div>
                )}

                <div className="flex-1">
                    <div className="flex items-center gap-2">
                        <Link to={`/companies/${company.id}`}>
                            <h3 className="font-semibold text-gray-900 transition group-hover:text-blue-600">
                                {company.name}
                            </h3>
                        </Link>
                        {company.isVerified && (
                            <CheckCircle className="h-5 w-5 text-blue-600" />
                        )}
                    </div>

                    {company.address && (
                        <p className="mt-1 flex items-center gap-1 text-sm text-gray-500">
                            <MapPin className="h-4 w-4" />
                            {company.address}
                        </p>
                    )}

                    {company.description && (
                        <p className="mt-2 line-clamp-2 text-sm text-gray-600">{company.description}</p>
                    )}
                </div>
            </div>

            {/* Tags */}
            <div className="mt-4 flex flex-wrap items-center gap-2">
                {company.isVerified && (
                    <Badge variant="success">Đã xác thực</Badge>
                )}
                {company.website && (
                    <a
                        href={company.website}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-1 text-sm text-blue-600 hover:underline"
                    >
                        <ExternalLink className="h-3 w-3" />
                        Website
                    </a>
                )}
            </div>

            {/* Footer */}
            <div className="mt-4 flex items-center justify-end border-t border-gray-100 pt-4">
                <Link to={`/companies/${company.id}`}>
                    <Button variant="ghost" size="sm">
                        Xem chi tiết
                    </Button>
                </Link>
            </div>
        </Card>
    );
}
