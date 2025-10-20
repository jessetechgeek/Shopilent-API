import type {NextConfig} from "next";

/** @type {import('next').NextConfig} */

const nextConfig: NextConfig = {
    reactStrictMode: true,
    // Enable standalone output for optimized Docker production builds
    output: 'standalone',
    webpack: (config, {dev, isServer}) => {
        if (dev) {
            config.watchOptions = {
                poll: 1000, // Check for changes every second
                aggregateTimeout: 300, // Delay before rebuilding
                ignored: ['**/node_modules', '**/.git', '**/.next'],
            };
        }
        return config;
    },
    turbopack: {
        // Explicitly tell Turbopack how to handle polling in Docker
        resolveExtensions: ['.tsx', '.ts', '.jsx', '.js', '.json'],
        // You can add more configuration as needed:
        // resolveAlias: {
        //   // Add any module aliases here
        // },
        // rules: {
        //   // Add any custom loaders here
        // }
    },
    onDemandEntries: {
        // period (in ms) where the server will keep pages in the buffer
        maxInactiveAge: 60 * 1000,
        // number of pages that should be kept simultaneously without being disposed
        pagesBufferLength: 5,
    }
};

export default nextConfig;
