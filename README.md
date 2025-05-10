# PollSpark

[![Build Status](https://github.com/YOUR_USERNAME/PollSpark/actions/workflows/build.yml/badge.svg)](https://github.com/YOUR_USERNAME/PollSpark/actions/workflows/build.yml)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

PollPulse is a fast, engaging web app for creating and voting in short polls, built to showcase modern C# and React skills. It features a clean, functional programming-inspired backend with Minimal APIs, MediatR, and union types, paired with a responsive React frontend. Key features include JWT authentication, poll creation, voting, real-time results, and data export for monetization.

## 🚀 Features
- **Create Polls**: Authenticated users can create polls with 2–4 options.
- **Vote**: Public or authenticated voting with one-click simplicity.
- **View Results**: Real-time results with percentage breakdowns, visualized in React with Recharts.
- **Authentication**: Secure JWT-based login/register using ASP.NET Core Identity.
- **Data Export**: Anonymized poll data for admins, enabling monetization for marketers.
- **Functional Programming**: Uses `OneOf` for union types, immutable `record` types, and pure MediatR handlers.

## 🛠️ Tech Stack
- **Backend**: C# (.NET 8), ASP.NET Core Minimal APIs, MediatR, Entity Framework Core, SQLite (dev)/PostgreSQL (prod), `OneOf` for union types.
- **Frontend**: React, Vite, React Router, React Hook Form, Recharts, Axios.
- **Tools**: Swagger for API documentation, GitHub for version control.

## 📦 Architecture
PollPulse follows a clean, modular architecture inspired by CQRS and functional programming:
- **Inputs**: Type-safe commands (`CreatePollCommand`) and validated user inputs.
- **Processing**: MediatR handlers for business logic, Minimal APIs for routing, EF Core for data access.
- **Outputs**: JSON responses (`PollDto`, `PollResultsDto`) and React-rendered UI.

## 🚦 Getting Started

### Prerequisites
- .NET 8 SDK
- SQLite (for development)
- Node.js (for frontend, coming soon)

### Backend Setup
```bash
cd src/backend/PollSpark
dotnet restore
dotnet ef database update
dotnet run
```

### API Documentation
Once running, visit `http://localhost:5000/swagger` for API documentation.

## 📝 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing
1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request
