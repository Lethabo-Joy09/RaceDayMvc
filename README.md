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

## Docker Setup Instructions

This application is containerized. To run it locally using Docker:

1. Ensure the **Part 2 API** is running on `http://localhost:5272`.
2. Build the Docker image:
   ```bash
   docker build -t racedaymvc:latest .
