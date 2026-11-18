# AppStudio migration inventory

## What this solution does

The solution is a workplace camera and people-operations system.  Its main
business capabilities are:

- **Unify.People** is the primary application.  It owns the PostgreSQL people
  data model and implements attendance, leave/regularisation, organisation and
  visitor/meeting workflows.
- **Unify.Camera** manages camera channels, cameras, intelligence feeds,
  monitoring, tickets, notifications and alert messages.
- **Unify.STR.Attendance** is a separate attendance integration for STR.  It
  imports/uploads attendance data and maintains STR-specific employee and
  organisation records.
- **UnifyNow.Forest** processes Forest events and rules, and writes operational
  logging/alert data.
- **Unify.Common** contains cross-service discovery URLs. **Toll.Web.UI** and
  **Whatsapp** are supporting UI/integration projects. **AppHost** and the
  Docker files are deployment/orchestration assets.

## People service business map

| Area | Controllers and implementation responsibility |
| --- | --- |
| Attendance | `AttendanceLogs`, `EmployeeActivity*`, `DoorActivity`, dashboards and STR dashboard query arrival/departure activity; `ApplyLeave`, `ApproveLeave`, `LeaveApproval` manage leave; `ApplyRegularization*` manages attendance corrections; `ReportEmployee` and `ReportDepartment` produce employee/department views. |
| Organisation | `Employee`, `EmployeeApp`, `Department`, `Designation`, `EmployeeType`, `Shift`, `LatePolicy`, `LeavePolicy`, `RegularizationPolicy` and `DoorStatus` maintain master data and employee policy assignment. |
| Visitor management | `Appointment`, `Meeting`, `CloseMeeting`, `Visitor`, `VisitorLog`, `MeetingPurpose`, `MeetingPlace` and `VisitorDashboard` manage the visitor lifecycle from appointment through meeting closure and reporting. |
| Processing and integrations | `AttendanceRulesEngine` turns camera inference events into attendance logs; `VisitorRulesEngine` is the equivalent visitor-event path; `InferenceServices`, `ConsumeServices`, `FaceService` and `AIPersonService` connect camera/AI and messaging services. |
| Dashboards | `WidgetService`, `DashboardServices` and the `Widgets` folder calculate KPI, chart and leaderboard data for attendance and visitor dashboards. |

## AppStudio dependency assessment

AppStudio has two very different roles in the current code:

1. **Infrastructure concerns**: static service lookup (`CoreServices`), current
   tenant/user values, EF attributes such as `AutoFill`, storage and messaging.
   These can be replaced with ASP.NET Core DI, claims, EF Core configuration,
   and explicit interfaces.
2. **Generated UI runtime**: `WebApp`, `PageController`, `FormView`, `GridView`,
   `UiAction`, menu registration and `AppStudio.Model` view models.  This is the
   runtime that creates virtually every People, Camera and STR screen.  It is
   not a package that can be removed while retaining those screens; each screen
   needs an MVC/Razor or API/UI replacement.

The native host in `Unify.People/Program.cs` is intentionally empty. It does
not register a database provider, AppStudio component, controller, endpoint or
view. `Unify.People.csproj` explicitly compiles only that host while the
AppStudio-generated source remains available as a migration reference. Native
service classes will be added explicitly once their dependencies are replaced.

## Required follow-up work

1. Introduce `ICurrentUserContext` backed by `IHttpContextAccessor` and replace
   static `CoreServices.CurrentUser` access.  Tenant isolation must be retained
   in every query.
2. Inject `PeopleDb`/`IDbContextFactory<PeopleDb>` and required business
   services into controllers and services.  Remove static `CoreServices.GetService`.
3. Move `AutoFill`, `RequiredValue`, `UIControl` and `ViewState` behavior into
   EF Core configuration, DTO validation and application code.
4. Replace each AppStudio page with a conventional controller/API endpoint and
   a Razor/SPA screen.  Start with organisation masters, then attendance, then
   visitor workflows and dashboards.
5. Replace AppStudio storage, messaging, identity and enterprise-data types
   with locally owned interfaces and ASP.NET Core Identity/messaging adapters.
6. Keep the legacy Camera, STR Attendance and Forest source excluded until its
   replacement is migrated. Their former framework package references have
   been removed and must not be restored to make legacy code compile.

## Target architecture

Each deployable application will be a **monolith with vertical business
slices**, organised into a conventional n-tier structure. The deployment unit
remains one application and one database boundary; it is not split into
microservices as part of this migration.

```
Host (ASP.NET Core composition root)
    └── Presentation (future HTTP endpoints/views)
            └── Application slices
                    ├── Attendance
                    ├── Organisation
                    └── Visitor management
                            └── Domain
                                    └── Infrastructure
```

- **Host** owns configuration and dependency registration only. It must not
  contain business rules, direct data access, or static service locators.
- **Presentation** is currently paused. When restored, it will translate HTTP
  requests to application commands/queries and return response DTOs; it will
  not contain workflow logic.
- **Application slices** own use cases such as recording attendance, approving
  leave, registering an employee, and opening/closing a visitor meeting. A
  slice defines its request/response DTOs and depends on interfaces, never on
  concrete data providers.
- **Domain** contains entities, value objects and business rules that do not
  depend on ASP.NET Core, EF Core, message brokers or UI code.
- **Infrastructure** supplies implementations for repositories, file storage,
  AI/camera clients and messaging. These are registered through dependency
  injection only when corresponding configuration exists.

This follows Microsoft ASP.NET Core guidance: use the composition root for DI,
use constructor injection, keep services focused and testable, avoid the
service-locator pattern, and keep controllers thin. Cross-cutting concerns
(logging, validation, authorization and exception handling) should be applied
through middleware, filters or pipeline behaviors rather than duplicated in
business services.

## Migration guardrails

- Do not add a database provider or connection string until a migrated
  infrastructure implementation needs it.
- Do not register controllers, map HTTP endpoints, configure generated views,
  or start broker consumers while their implementation remains legacy code.
- Migrate one slice end-to-end: domain types, application service/contracts,
  infrastructure adapter, tests, and only then its presentation endpoint.
- Keep the legacy source excluded from compilation until the equivalent native
  slice has tests and is ready to replace it. Do not reintroduce its framework
  packages as a shortcut.

## Native migration baseline

`Unify.People` now compiles only its native migration baseline. It has no
mapped HTTP endpoints or Razor compilation. The active code contains:

- request DTOs for organisation, attendance and visitor workflows;
- framework-free domain models for employees, departments, attendance entries
  and visitor appointments;
- validation services for those commands, registered by constructor-based DI;
- persistence repository contracts only—there is no EF Core, SQL Server,
  connection string or provider dependency yet.

The following legacy concerns remain deliberately paused: generated page/grid
controllers and Razor files; DevExtreme data loading; Excel/PDF exports; face
recognition; RabbitMQ message handling; Elasticsearch dashboards; and all
database-backed implementations. They should each be reintroduced only behind
an application contract once the relevant native slice is ready.

## Remaining project baselines

| Project | Native foundation retained | Deferred legacy concerns |
| --- | --- | --- |
| `Unify.CCC` | Camera registration and monitor-configuration contracts, model and validation | Camera persistence, AI feeds, alerts/tickets, controllers and generated UI |
| `Unify.STR.Attendance` | Attendance-import contract, model and validation | Upload endpoints, Excel, geospatial checks, face recognition and persistence |
| `UnifyNow.Forest` | Alert-rule contract, model and validation | Camera/AI integrations, RabbitMQ, Elasticsearch, messaging, controllers and UI |
| `Unify.Common` | Environment-aware discovery service | Identity, account pages, EF Core, authentication providers and logging packages |
| `Toll.Web.UI` | Vehicle-registration contract, model and validation | AppBuilder runtime, SQL/Identity, exports, grid UI, controllers and views |
| `AppHost` | Package-free orchestration placeholder | Aspire, service discovery, containers and database provisioning |

The legacy `docker-compose` project is intentionally not included in the
solution. It requires Visual Studio Container Tools and Docker Desktop, while
no migrated application currently declares a container, database or external
runtime resource. The compose files remain available for a later deployment
phase.

All of these projects compile only their native baseline. As with People, the
next step is to add tests and then introduce infrastructure through repository
or gateway contracts—not to reactivate the legacy controller/view code.
