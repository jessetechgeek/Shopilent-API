FROM node:20-alpine

WORKDIR /app

# Copy package files first for better caching
COPY package*.json ./

# Install dependencies (use npm install for dev instead of npm ci)
RUN npm install

# Copy the rest of the application
# In development mode, this will be overridden by the volume mount
COPY . .

# Expose Vite dev server port as configured in vite.config.js
EXPOSE 5173

# Start dev server with HMR
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]