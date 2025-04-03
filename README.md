# Weather Plugin for Power Platform

This repository contains a custom C# plugin for Microsoft Dataverse.

🧩 Features:
- Converts temperature between Celsius and Fahrenheit
- Determines weather status (Warmer / Colder / No Change)
- Manages active weather record (only one active at a time)
- Tracks change counter and resets at 10

📦 To register this plugin:
- Use Plugin Registration Tool
- Register on `Create` of `WeatherReport` table
- Execution: PostOperation, Synchronous

🛠️ Developed as part of a Power Platform project (Matrix weather app).
