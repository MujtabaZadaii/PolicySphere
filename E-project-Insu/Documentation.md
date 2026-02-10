# PolicySphere - Project Documentation

## Overview
PolicySphere is a modern, premium insurance management platform designed to provide a seamless digital experience for policyholders and administrators. Built with ASP.NET Core MVC, it leverages a high-end, cinematic dark-themed UI/UX to build trust and engagement.

## Technology Stack
- **Framework**: ASP.NET Core MVC (C#)
- **Database**: SQL Server (Entity Framework Core)
- **Styling**: Tailwind CSS (via CDN)
- **Animations**: GSAP (GreenSock Animation Platform), Lenis (Smooth Scroll)
- **Frontend Logic**: Vanilla JavaScript
- **Fonts**: Inter, Oswald, Playfair Display (Google Fonts)

## Key Features

### 1. User Interface (UI) & User Experience (UX)
- **Cinematic Design**: A completely custom dark mode aesthetics with neon purple (#AC58E9) accents.
- **Micro-Interactions**: Smooth hover effects, scroll-triggered animations (fade-ins, reveals), and a "tubes" cursor effect in the footer.
- **Responsiveness**: Fully responsive layout using Tailwind CSS grid and flexbox utilities.

### 2. Core Functionalities
- **User Authentication**: Secure Login and Signup flows for both Users and Admins.
- **Role-Based Access Control**:
    - **Admin**: Dashboard access to manage users, policies, and view analytics.
    - **User**: Dashboard to view active policies, apply for loans, and calculate premiums.
- **Policy Management**:
    - Detailed pages for Life, Medical, Motor, and Home insurance.
    - Interactive "Journey" calculator to estimate premiums based on age, coverage type, and sum assured.

### 3. Project Structure
- **Controllers**: Handles business logic (e.g., `HomeController`, `UserController`, `AdminController`).
- **Views**: Razor views (`.cshtml`) for rendering HTML.
    - `Home/`: Public facing pages (Index, About, Contact).
    - `Shared/`: Layouts and partials (`_Layout.cshtml`).
- **wwwroot**: Static assets (CSS, images, videos).

## Installation & Setup
1. **Prerequisites**: .NET 8.0 SDK (or compatible version).
2. **Database Setup**: Ensure connection string in `appsettings.json` points to a valid SQL Server instance. Apply migrations if necessary.
3. **Run**: Use `dotnet run` or start via Visual Studio.

## Recent Updates
- **Hero Section**: Enhanced with a premium abstract background visual.
- **Footer**: Integrated tube cursor animation and premium layout.
- **Navbar**: Optimized for transparency and glassmorphism on scroll.
