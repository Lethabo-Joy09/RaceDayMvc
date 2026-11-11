# RaceDay MVC Web Application (Part 3)

## Overview
This is the ASP.NET Core MVC web application for the RaceDay event management platform. It consumes the Part 2 RESTful API to provide a complete, role-based interface for Organisers and Participants. 

## User Roles
*   **Organiser**: Can create, edit, and delete events. Can create race categories, view event enrolments, and capture finish times/results.
*   **Participant**: Can browse upcoming events, enrol in specific event categories, view their enrolment history, and track their personal race results.

## Features Implemented
*   **Authentication**: Login, Registration, and Session Management using API cookies.
*   **Role-Based Navigation**: Distinct dashboards and menus for Organisers and Participants.
*   **Organiser Dashboard**: Full CRUD for Events, Category Management, Enrolment Viewing, and Result Capture.
*   **Participant Dashboard**: Event Browsing, Enrolment, My Enrolments, and My Results.
*   **Containerisation**: The application is fully containerized using Docker.

## Technologies Used
*   ASP.NET Core MVC (.NET 8.0)
*   `HttpClient` for API communication
*   Session-based authentication
*   Docker
*   Bootstrap 5 for styling
  ## Proof of Working Application

### Docker Containers Running
<img width="1366" height="768" alt="Docker png" src="https://github.com/user-attachments/assets/a7e4f8ed-ac05-45c0-894f-ddb0d9244fa4" />
### CI/CD Green Build
<img width="1366" height="768" alt="Screenshot 2026-11-11 145949" src="https://github.com/user-attachments/assets/17ad02ca-4c1b-4f7f-9ee3-bfaa8d132fd3" /> 
### Visual Studio Container View
<img width="1366" height="768" alt="container png" src="https://github.com/user-attachments/assets/b88f30b2-d50f-4c19-a173-19a5cb3bebf6" />





This application is containerized. To run it locally using Docker:

1. Ensure the **Part 2 API** is running on `http://localhost:5272`.
2. Build the Docker image:
   ```bash
   docker build -t racedaymvc:latest .
