The Little Issue Lite Web App
The Little Issue Lite Web App is a lightweight web application created by VCV Solutions for The Little Issue Magazine. Designed as an MVP (Minimum Viable Product), the app serves as a prototype to showcase core functionality and user experience, providing users with easy access to educational articles and resources. This Lite version is geared towards attracting potential development partners or investors interested in building a full-featured platform for The Little Issue.

Table of Contents
•	Project Overview
•	Features
•	Tech Stack
•	Architecture
•	Getting Started
•	Usage
•	Deployment
•	Security
•	Contributing
•	License
•	Contact

Project Overview
The Little Issue Lite Web App allows readers to browse and read educational articles in a secure, accessible, and child-friendly environment. Core functionality includes browsing articles by category, reading and downloading PDFs, and personalized user profiles. This application, hosted on Google Cloud, serves as a proof of concept to demonstrate the potential of a full-featured digital platform in the future.
Note: As this app was developed for an NPO, it is intended to be free for all users.


Features
•	User Authentication: Login and registration with child-safe authentication for young users and parental control options.
•	Browse Articles: Filter articles by category, with a focus on educational content.
•	Read and Download: Read articles in-app, or download them as PDFs for offline access.
•	User Profile: View saved articles, update account settings, and access previous downloads.
•	Admin & Content Management: Moderation tools for editors to publish content, manage users, and oversee interactions.

Tech Stack
•	Frontend: ASP.NET MVC, HTML, CSS, JavaScript
•	Backend: .NET Framework (Visual Studio)
•	Database: Firestore (Google Cloud)
•	Storage: Google Cloud Storage for PDF assets
•	DevOps: GitHub Actions for CI/CD

Architecture
1. Design Patterns
•	Model-View-Controller (MVC): Provides a clean separation of concerns, enhancing modularity and maintainability.
•	Repository Pattern: Used for managing data access, enabling clear abstraction and easier testing.
2. Architecture Patterns
•	Client-Server Architecture: Ensures scalability and central control over data.
•	Cloud-Based Serverless Infrastructure: Hosted on Google Cloud to simplify deployment, security, and scalability.
3. Cloud
•	Google Cloud Console hosts the Firestore NoSQL database and Google Cloud Storage, providing secure and scalable data handling with automatic backups.

Getting Started
Prerequisites
1.	.NET Framework: Required for backend development.
2.	Visual Studio: For application code management.
3.	Google Cloud Console Access: Access Firestore database and storage bucket.
Installation
1.	Clone the Repository:
bash
Copy code
git clone https://github.com/kurtsiebritz/TheLittleIssue.git
cd TheLittleIssue
2.	Set Up Environment Variables:
o	Load/copy the Credentials .json file to the appsettings folder in the project directory.
o	Either request access from developers.
o	If running with access to all project files, the .json file is available in the applications folder, along with a zip version of github.
3.	Build and Run:
o	Open in Visual Studio.
o	Build the solution and run the app locally.

Usage
•	Articles: Browse, search, and read educational articles by topic.
•	Interactive Activities: Engage with games, puzzles, and quizzes.
•	Reading Recommendations: Personalized book and resource recommendations.
•	Parent/Teacher Resources: Access downloadable resources like lesson plans and educational tips.
•	User Submission: Users can submit stories or artwork.


Deployment
The application is deployed via GitHub Actions and hosted on Google Cloud. Key deployment actions include:
1.	CI/CD Pipeline:
o	GitHub Actions tests code on each push to ensure stability.
o	Successful builds trigger automatic deployments to Google Cloud.
2.	Google Cloud Hosting:
o	Firestore and Google Cloud Storage for data and media assets.

Security
•	Data Protection: All sensitive data is encrypted during transit and at rest.
•	Child-Safe Authentication: Options for guardian approval and child-safe login.
•	Content Moderation: Built-in moderation for user-submitted content to ensure compliance.


Contributing
We welcome contributions to enhance The Little Issue Lite Web App. Please submit a pull request or open an issue in the GitHub repository to discuss potential changes.


License
This project is licensed under the MIT License. See the LICENSE file for details.


Contact
For support or further information, please contact:
•	VCV Solutions Team Leader: Sameer Rajie - ST10208353@vcconnect.edu.za
•	VCV Solutions Support: support@vcvsolutions.com
