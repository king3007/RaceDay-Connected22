# RaceDay — connected MVC and API

## Local image uploads — use wwwroot, not Azure

Profile photos and event banners are uploaded to the **MVC application**, using `multipart/form-data` forms and `IFormFile` inputs. No Azure or cloud image service is used.

- Banner files: `RaceDay_MVC/wwwroot/uploads/banners/`
- Profile photos: `RaceDay_MVC/wwwroot/uploads/profiles/`
- Private image-to-event/user references: `RaceDay_MVC/App_Data/image-references/`

Run the app from a writable folder. Retain **both wwwroot/uploads and App_Data** when moving or updating the application; they contain your saved images and their references. The MVC application serves the images with `UseStaticFiles()`. Uploaded photos are public static assets and should be pictures users intend to display.

The API remains responsible for users, events and other business data. Image files and their references are managed locally by MVC, so this change does **not** need new database columns or migrations. Profile photos are associated with the signed-in API user ID; banners are associated with the event ID returned by event creation. Other clients of the API do not receive these MVC-local image paths.

### Try the uploads

1. Log in as Organiser. Open **Create event**, select a banner, review the preview, and submit. Confirm the banner appears in the organiser overview and the public **Explore events** page.
2. Click **Update banner** on an owned event to upload a replacement. A missing/failed banner can be retried here without creating another event.
3. Open **My profile** from either an Organiser or Participant account. Select a profile photo and save. The image appears on the profile page and in the dashboard sidebar.
4. Refresh the page, log out/in and restart MVC. Existing images should still load. Saving profile details without choosing another file keeps the existing photo.
5. Try a text file renamed to `.jpg` or a file above 5 MB: it should be rejected. Supported formats are JPG/JPEG, PNG and WebP. Server-generated filenames prevent client filenames from becoming disk paths. Requests above the total 6 MB request limit are rejected by the server.
6. Sign in as another organiser: they must not be able to update the first organiser's banner. The banner controller verifies event ownership through `/api/events/mine` before writing.

Image extension and file-signature checks run on the server. Files are not decoded or resized. Browser previews help users check the selected image before saving. A new filename is used on replacement so browsers do not keep showing a cached older photo.

## Run in Visual Studio

1. Extract the entire ZIP. Open `RaceDay-Connected.sln` in Visual Studio 2022 with the .NET 8 SDK and ASP.NET/web development workload installed.
2. In `RACEDAY_API_POE/appsettings.json`, check `ConnectionStrings:RaceDayConnection`. It still contains the SQL Server configuration supplied in your API. Update it for your own SQL Server instance and make sure that server is running. The API calls `EnsureCreated()` on startup to create a new database/tables when required; it does not migrate an existing incompatible database.
3. Trust your development HTTPS certificate. Run `dotnet dev-certs https --trust` in a terminal if necessary.
4. Right-click the solution → Configure Startup Projects → Multiple startup projects. Set both `RACEDAY_API_POE` and `RaceDay_MVC` to **Start**, with the **https** launch profile for each. A shared launch profile is also included for recent Visual Studio versions.
5. Start the solution. API Swagger: https://localhost:7152/swagger. Website: https://localhost:7087.
6. Register an Organiser and create an event. Open Categories and add a category. Log out, register a Participant and enrol in that category. Switch back to the Organiser to view enrolments and capture a result (finish time `hh:mm:ss`). The Participant can then view it under My results.

The database server must be available before the API starts. If using IIS Express instead of the **https** project profile, set `RaceDay_MVC/appsettings.json` → `RaceDayApi:BaseUrl` to the API address shown in your browser (the supplied IIS Express API uses a different port).

## Run from two terminals instead

From the extracted solution folder:

```powershell
dotnet restore RaceDay-Connected.sln
dotnet dev-certs https --trust
dotnet run --project RACEDAY_API_POE/RACEDAY_API_POE.csproj --launch-profile https
```

In a second terminal:

```powershell
dotnet run --project RaceDay_MVC/RaceDay_MVC.csproj --launch-profile https
```

## Connected functionality

- Registration maps FirstName, LastName, Email, Password, PhoneNumber, Role, DateOfBirth and Gender to the API. Confirmation is validated in MVC.
- Login reads token, expiry and nested user/role. MVC stores an encrypted, signed authentication cookie and forwards the API bearer token on server-to-server requests. Role-protected pages use the authenticated principal.
- Event listing and calendar display live API data. Public results directs users to their permitted results pages because the supplied API has no public results endpoint.
- Event creation sends JSON Name, Description and EventDate to the API, then saves the optional banner locally under wwwroot using the returned eventId. Location, distance and event type inputs are omitted because this API does not store them. Race type can be entered in categories.
- Organiser overview lists owned events; organisers can add categories, view enrolments and record/view event results.
- Participants can browse/enrol and view their enrolments/results. Both roles share My profile for first/last-name changes and local profile photo uploads; organisers can also edit their phone number.
- API failures show a clear error page; expired/unauthorised API sessions return users to login. All MVC write forms validate antiforgery tokens.

## Small API adjustments included

- Added authorised `GET /api/events/mine` to return only the current organiser's events.
- Enrolment now verifies the chosen category belongs to the event.
- Corrected the default gender fallback to `Other` so it fits the model's 10-character limit.

The API's database settings and JWT configuration were retained from your upload. No database or package restore was run here. Other original API endpoints remain available. The contact page is still the original presentation form; the API has no contact-message endpoint. Event edit/delete endpoints are not surfaced in this MVC version.

## Verification

Source-level checks verify API routes, payload field names, login response mapping, event/category identifiers, role guards, antiforgery coverage, multipart upload forms, image persistence paths, JSON configuration and solution paths. JavaScript preview behaviour is checked with a mocked DOM (valid/invalid selection, maximum size, replacement and reset). This environment has no .NET SDK, SQL Server or browser executable, so compilation, browser layout and live end-to-end execution were not verified. Run the organiser → participant → result walkthrough above after configuring your database.
