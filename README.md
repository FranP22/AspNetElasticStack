# Elastic Observability + .NET Microservice Stack

A Docker-based full-stack infrastructure featuring:

- .NET backend API
- PostgreSQL database
- Nginx reverse proxy
- Elastic Stack (Elasticsearch, Kibana, Logstash)
- Filebeat + Metricbeat for logs and metrics

The system is designed for centralized logging, monitoring, and SIEM-style observability.

## Architecture Overview

Client
  │
  ▼
NGINX (Reverse Proxy)
  │
  ▼
.NET API (myapp)
  │
  ├──────────────► PostgreSQL (data storage)
  │
  └──────────────► Logging & Metrics Pipeline
                      │
        ┌─────────────┴─────────────┐
        ▼                           ▼
Filebeat / Logstash             Metricbeat
        │                           │
        └─────────────┬─────────────┘
                      ▼
                Elasticsearch
                      ▼
                   Kibana UI
				   
## Features

- TLS-secured Elasticsearch cluster
- Kibana dashboards for log analysis
- Centralized logging (Filebeat + Logstash)
- System + container metrics (Metricbeat)
- Nginx reverse proxy entrypoint
- PostgreSQL persistence
- Fully Dockerized environment
- Isolated networks (api / database / elastic)

## Running the Project

1. Clone repository
```
git clone https://github.com/your-repo/your-project.git
cd your-project
```

2. Create environment file
```
cp .env.example .env
```

3. Start stack
```
docker compose up -d --build
```

## Access Services

- API - http://localhost
- Kibana - http://localhost:${KIBANA_PORT}
- Elasticsearch -	https://localhost:${ES_PORT}

## Purpose

Production-style observability stack:
- Centralized logging (SIEM-like)
- Infrastructure monitoring
- Secure Elastic Stack deployment
- Microservice architecture foundation

## Credits

This project is based on and extended from:
- This repository: https://github.com/elkninja/elastic-stack-docker-part-one  
- Original blog post: https://www.elastic.co/blog/getting-started-with-the-elastic-stack-and-docker-compose