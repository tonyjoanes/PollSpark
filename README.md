# PollSpark

[![Build Status](https://github.com/tonyjoanes/PollSpark/actions/workflows/build.yml/badge.svg)](https://github.com/tonyjoanes/PollSpark/actions/workflows/build.yml)
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

## 🎯 Planned Features

### Enhanced Poll Types
- Multiple choice polls
- Ranked choice voting
- Time-based polls
- Image-based polls
- Poll templates for common use cases

### Social Features
- Share polls on social media
- Embed polls on external websites
- Follow other users
- Comment system
- Poll reactions (like, bookmark)

### Advanced Analytics
- Demographic breakdown of votes
- Time-based voting patterns
- Export results in various formats
- Custom charts and visualizations
- Vote history tracking

### User Experience
- Dark mode support
- Mobile-optimized interface
- Keyboard shortcuts
- Poll preview before publishing
- Draft saving functionality
- Poll scheduling

### Engagement Features
- Poll of the day/week
- Featured polls section
- User reputation system
- Poll creation streaks
- Achievement badges

### Privacy and Control
- Private polls (invite-only)
- Password-protected polls
- IP-based voting restrictions
- Email verification for voting
- Poll expiration settings

### Integration Features
- Calendar integration
- Email notifications
- Webhook support
- API access for developers
- Export/import functionality

### Moderation Tools
- Report inappropriate polls
- Content moderation queue
- User blocking
- Poll review system
- Automated content filtering

### Monetization Options
- Premium features for power users
- Sponsored polls
- Advanced analytics for businesses
- Custom branding options
- API access tiers

### 🤖 AI-Powered Features
- **Smart Poll Suggestions**: AI-generated poll recommendations based on user interests and trending topics
- **Content Moderation**: AI-powered detection of inappropriate content in polls and comments
- **Sentiment Analysis**: Analyze voter sentiment and provide insights on poll results
- **Smart Scheduling**: AI recommendations for optimal poll timing based on audience engagement patterns
- **Automated Summaries**: Generate concise summaries of poll results and key insights
- **Trend Prediction**: Predict potential poll outcomes based on early voting patterns
- **Personalized Experience**: AI-driven content recommendations and user experience customization
- **Natural Language Polls**: Create polls from natural language descriptions
- **Smart Categories**: Automatic categorization and tagging of polls using AI
- **Voter Insights**: AI-powered analysis of voting patterns and demographic trends

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
