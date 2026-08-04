# File & Image Sharing Service

A full-stack file and image sharing service developed for **AMD201 — Advanced Microservices Deployment, Topic 3**.

The application allows users to upload files, receive a unique short sharing link, preview supported images, download files, view upload history, configure expiry/download limits, and delete uploaded files.

The system is built using **ASP.NET Core Web API**, **Vue 3**, **MySQL**, **Cloudinary**, **Docker**, and **GitHub Actions**.

---

# 1. Live Application

### Frontend

https://filesharingsevice-production-eb82.up.railway.app

### Backend API

https://b-e.up.railway.app

### Swagger API Documentation

https://b-e.up.railway.app/swagger

### Repository

https://github.com/Kiblue0919/FileSharingSevice

---

## 2. Features

### Core Features

- Upload files up to 10 MB
- Drag-and-drop file upload
- File picker upload
- Unique short sharing code generated for each upload
- Shareable file links
- File metadata retrieval
- File download
- Inline image preview
- Upload history
- File deletion
- File size validation
- Server-side MIME type validation
- Expiry options:
  - 1 hour
  - 1 day
  - 1 week
  - Never
- Download limits
- Automatic handling of expired files
- Automatic cleanup of expired files through a background service

### Additional Features

- Cloudinary cloud storage
- Upload progress bar
- Swagger API documentation
- Multi-stage Docker build
- GitHub Actions CI pipeline
- Automated backend/frontend build checks
- Unit tests using xUnit, Moq and FluentAssertions

---

# 3. Technology Stack

## Backend

| Technology | Purpose |
|---|---|
| ASP.NET Core .NET 8 | REST API |
| Entity Framework Core 8 | ORM / database access |
| Pomelo.EntityFrameworkCore.MySql | MySQL provider |
| CloudinaryDotNet | Cloud file storage |
| Swashbuckle.AspNetCore | Swagger API documentation |
| xUnit | Unit testing |
| Moq | Dependency mocking |
| FluentAssertions | Test assertions |

## Frontend

| Technology | Purpose |
|---|---|
| Vue 3 | SPA framework |
| Vue Router | Client-side routing |
| Vite | Development server and build tool |
| Axios | HTTP API communication |

## Infrastructure / DevOps

| Technology | Purpose |
|---|---|
| GitHub | Source control |
| GitHub Actions | Continuous Integration |
| Docker | Backend containerisation |
| Railway | Cloud deployment |
| MySQL | Relational database |
| Cloudinary | Cloud file storage |
| Postman | API testing |
| Visual Studio 2022 | Backend development |
| VS Code | Frontend development |

---

# 4. System Architecture

The system uses a three-tier architecture.

```text
                         USER
                           |
                           | HTTP / HTTPS
                           v
                +-----------------------+
                |     Vue 3 Frontend    |
                |       Vite SPA        |
                +-----------+-----------+
                            |
                            | REST API
                            v
              +-----------------------------+
              |     ASP.NET Core Web API    |
              |          .NET 8             |
              +-------------+---------------+
                            |
              +-------------+-------------+
              |                           |
              v                           v
      +---------------+          +------------------+
      |    MySQL      |          |    Cloudinary    |
      |   Database    |          |   Cloud Storage  |
      +---------------+          +------------------+
              |
              |
       File Metadata
       
Cloudinary stores the actual uploaded file bytes.
MySQL stores file metadata and application state.
