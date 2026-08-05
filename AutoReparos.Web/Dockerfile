# Multi-stage build for Angular with Node and Nginx
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile --ignore-scripts

COPY . .
RUN yarn build --configuration production

FROM nginx:alpine
COPY --from=build /app/dist/app-temp/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh && \
    chown -R nginx:nginx \
      /usr/share/nginx/html \
      /var/cache/nginx \
      /var/log/nginx \
      /etc/nginx/conf.d \
      /docker-entrypoint.sh && \
    touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid

USER nginx

EXPOSE 8080

ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
