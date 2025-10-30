# ==============================================
# Dockerfile para GranjaTech Frontend (React)
# ==============================================

# Estágio 1: Build
FROM node:20-alpine AS build

WORKDIR /app

# Copia package files para cache eficiente
COPY package.json package-lock.json ./

# Instala dependências
RUN npm ci --silent

# Copia código fonte
COPY . .

# Define a URL da API para o build
ENV REACT_APP_API_URL=http://localhost:5099/api

# Build da aplicação React
RUN npm run build

# Estágio 2: Production com Nginx
FROM nginx:alpine AS production

# Copia build para o Nginx
COPY --from=build /app/build /usr/share/nginx/html

# Copia configuração customizada do Nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expõe porta
EXPOSE 80

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:80 || exit 1

CMD ["nginx", "-g", "daemon off;"]
