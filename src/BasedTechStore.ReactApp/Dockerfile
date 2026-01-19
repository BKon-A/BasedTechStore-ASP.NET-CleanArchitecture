FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

FROM node:22-alpine AS production

WORKDIR /app

COPY --from=build /app/dist ./dist
# COPY --from=build /app/public ./public
COPY --from=build /app/package*.json ./
COPY --from=build /app/vite.config.ts ./

RUN npm ci --omit=dev && npm cache clean --force

# FROM nginx:alpine
# COPY --from=build /app/dist /usr/share/nginx/html
# COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 3000
CMD ["npm", "run", "preview"]