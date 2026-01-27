import { useState } from 'react';
import { ChevronLeft, ChevronRight, MapPin, Briefcase } from 'lucide-react';
import { Button } from './Button';

interface JobBanner {
    id: number;
    title: string;
    company: string;
    location: string;
    salary: string;
    image: string;
}

interface CarouselProps {
    items: JobBanner[];
}

export function Carousel({ items }: CarouselProps) {
    const [currentSlide, setCurrentSlide] = useState(0);

    const nextSlide = () => {
        setCurrentSlide((prev) => (prev + 1) % items.length);
    };

    const prevSlide = () => {
        setCurrentSlide((prev) => (prev - 1 + items.length) % items.length);
    };

    return (
        <div className="relative mb-12 overflow-hidden rounded-2xl">
            <div className="relative h-64 sm:h-80 lg:h-96">
                {items.map((banner, index) => (
                    <div
                        key={banner.id}
                        className={`absolute inset-0 transition-opacity duration-500 ${
                            index === currentSlide ? 'opacity-100' : 'opacity-0'
                        }`}
                    >
                        <img
                            src={banner.image}
                            alt={banner.title}
                            className="h-full w-full object-cover"
                        />
                        <div className="absolute inset-0 bg-gradient-to-r from-black/60 to-transparent" />
                        <div className="absolute bottom-6 left-6 right-6 text-white">
                            <h3 className="text-xl font-bold sm:text-2xl">{banner.title}</h3>
                            <p className="text-lg opacity-90">{banner.company}</p>
                            <div className="mt-2 flex items-center gap-4 text-sm">
                                <span className="flex items-center gap-1">
                                    <MapPin className="h-4 w-4" />
                                    {banner.location}
                                </span>
                                <span className="flex items-center gap-1">
                                    <Briefcase className="h-4 w-4" />
                                    {banner.salary}
                                </span>
                            </div>
                            <Button className="mt-4 bg-white text-[#00b14f] hover:bg-gray-100">
                                Ứng tuyển ngay
                            </Button>
                        </div>
                    </div>
                ))}
            </div>

            {/* Navigation Buttons */}
            <button
                onClick={prevSlide}
                className="absolute left-4 top-1/2 -translate-y-1/2 rounded-full bg-white/20 p-2 text-white backdrop-blur-sm transition hover:bg-white/30"
            >
                <ChevronLeft className="h-6 w-6" />
            </button>
            <button
                onClick={nextSlide}
                className="absolute right-4 top-1/2 -translate-y-1/2 rounded-full bg-white/20 p-2 text-white backdrop-blur-sm transition hover:bg-white/30"
            >
                <ChevronRight className="h-6 w-6" />
            </button>

            {/* Indicators */}
            <div className="absolute bottom-4 left-1/2 flex -translate-x-1/2 gap-2">
                {items.map((_, index) => (
                    <button
                        key={index}
                        onClick={() => setCurrentSlide(index)}
                        className={`h-2 w-2 rounded-full transition ${
                            index === currentSlide ? 'bg-white' : 'bg-white/50'
                        }`}
                    />
                ))}
            </div>
        </div>
    );
}