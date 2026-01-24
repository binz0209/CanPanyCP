import * as React from 'react';
import { cn } from '@/utils';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'default' | 'destructive' | 'outline' | 'secondary' | 'ghost' | 'link';
    size?: 'default' | 'sm' | 'lg' | 'icon';
    isLoading?: boolean;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
    ({ className, variant = 'default', size = 'default', isLoading, disabled, children, ...props }, ref) => {
        const baseStyles = 'inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-lg font-semibold transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50';

        const variants = {
            default: 'bg-gradient-to-r from-[#00b14f] to-[#00a045] text-white hover:from-[#00a045] hover:to-[#008f3c] shadow-md hover:shadow-lg hover:-translate-y-0.5 focus-visible:ring-green-500',
            destructive: 'bg-red-600 text-white hover:bg-red-700 focus-visible:ring-red-500',
            outline: 'border-2 border-[#00b14f] bg-white text-[#00b14f] hover:bg-[#00b14f] hover:text-white focus-visible:ring-green-500',
            secondary: 'bg-gray-100 text-gray-900 hover:bg-gray-200 focus-visible:ring-gray-500',
            ghost: 'text-gray-700 hover:bg-gray-100 hover:text-[#00b14f] focus-visible:ring-gray-500',
            link: 'text-[#00b14f] underline-offset-4 hover:underline focus-visible:ring-green-500',
        };

        const sizes = {
            default: 'h-10 px-5 py-2 text-sm',
            sm: 'h-9 px-4 text-xs',
            lg: 'h-12 px-8 text-base',
            icon: 'h-10 w-10',
        };

        return (
            <button
                className={cn(baseStyles, variants[variant], sizes[size], className)}
                ref={ref}
                disabled={disabled || isLoading}
                {...props}
            >
                {isLoading && (
                    <svg className="h-4 w-4 animate-spin" viewBox="0 0 24 24" fill="none">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                    </svg>
                )}
                {children}
            </button>
        );
    }
);
Button.displayName = 'Button';

export { Button };
