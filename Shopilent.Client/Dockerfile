FROM node:20-alpine

# Set working directory
WORKDIR /app

# Copy package files
COPY package.json package-lock.json* ./

# Install dependencies
RUN npm install

# We'll mount the source code as a volume, so we don't need to copy it here
# COPY . .

# Expose the development port
EXPOSE 3000

# Enable polling for file changes in Docker
ENV WATCHPACK_POLLING=true
ENV CHOKIDAR_USEPOLLING=true

# Start Next.js in development mode with hot reload
# The -p flag ensures changes are picked up
CMD ["npm", "run", "dev", "--", "--port", "3000"]