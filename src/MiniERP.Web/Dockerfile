FROM node:18-alpine AS build
WORKDIR /app
COPY src/MiniERP.Web/package*.json ./
COPY src/MiniERP.Web/tsconfig*.json ./
RUN npm install --production=false
COPY src/MiniERP.Web/ ./
RUN npm run build

FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
