# KidsPocket – פלטפורמת חינוך פיננסי (MVP v1)

נבנה לפי האפיון המלא: כל כניסת כסף עוברת דרך **Decision Flow** (לא הפקדה אוטומטית),
היתרה תמיד נגזרת מ-**Ledger** append-only (לא מאוחסנת), ותמיכה במשפחות מרובות בתי אב.

## מבנה הפתרון

```
KidsPocket.sln
src/
  KidsPocket.Domain          - Entities + Domain Services (DecisionEngine, LedgerCalculator). אפס תלויות חיצוניות.
  KidsPocket.Contracts       - DTOs משותפים בין Api ל-Web (Blazor)
  KidsPocket.Infrastructure  - EF Core, DbContext, זריעת קופות ברירת המחדל
  KidsPocket.Application     - Vertical Slice Architecture: Features/<Domain>/<UseCase>
  KidsPocket.Api             - ASP.NET Core Web API
  KidsPocket.Web             - Blazor Web App (שלד ראשוני בלבד)
tests/
  KidsPocket.Domain.Tests       - טסטים אמיתיים ל-DecisionEngine ול-Decision
  KidsPocket.Application.Tests  - שלד + TODO
  KidsPocket.Api.Tests          - שלד + TODO
```

## עקרונות ארכיטקטוניים שנאכפים בקוד

1. **Ledger-only balance** - `Wallet`/`Child` לא מחזיקים שדה יתרה. `LedgerCalculator` מחשב הכל
   מתוך `LedgerEntry` בכל בקשה. `LedgerEntry` הוא Append-only - אין לו מתודות Update.
2. **הכל עובר דרך Decision** - אין endpoint שמזכה קופה ישירות. `MoneyEvent` (מכל מקור - דמי כיס,
   Chore, מתנה, תגמול) תמיד יוצר `Decision` עם הצעת ברירת מחדל של 10%/10%/10%, וההקצאה הסופית
   נשמרת רק דרך `POST /api/decisions/{id}/allocate`.
3. **קופות כטבלה, לא Enum** - `Bucket` היא Entity עם `Code` ייחודי, לא enum קשיח, כדי לתמוך
   בעתיד ב-Donation/Education/Travel/קופות מותאמות אישית בלי מיגרציית סכימה.
4. **Household נפרד מ-Child** - `ChildAdult` מקשר ילד לכמה בתי אב (הורים גרושים וכו'), לילד יש
   `Ledger` אחד יחיד ללא קשר לכמות בתי האב.
5. **Vertical Slice** - כל Use Case תחת `Features/<Domain>/<UseCase>/` עם Command/Query + Handler
   משלו. בלי MediatR (כדי לא להוסיף תלות NuGet לא ודאית בסביבה בלי אינטרנט) - `ICommandHandler`/
   `IQueryHandler` קלים משלנו ב-`Abstractions`.

## איך להריץ אצלך

```bash
cd KidsPocket
dotnet restore
```

צריך PostgreSQL מקומי (או להחליף provider ב-Infrastructure.csproj + Program.cs).
עדכן connection string ב-`src/KidsPocket.Api/appsettings.json`.

```bash
cd src/KidsPocket.Api
dotnet tool install --global dotnet-ef   # אם אין לך עדיין
dotnet ef migrations add InitialCreate --project ../KidsPocket.Infrastructure --startup-project .
dotnet ef database update --project ../KidsPocket.Infrastructure --startup-project .
dotnet run
```

Swagger: `https://localhost:xxxx/swagger`

## Happy Path לבדיקה ידנית

1. `POST /api/households` → `{ "name": "משפחת כהן" }`
2. `POST /api/households/{id}/adults` → הורה
3. `POST /api/households/{id}/children` → ילד (יוצר גם Ledger)
4. `POST /api/money-events` → `{ childId, source: "WeeklyAllowance", amount: 100 }` → מחזיר `decisionId`
5. `GET /api/decisions/{decisionId}/pending` → רואים את הצעת ה-10/10/10 וה-remainder
6. `POST /api/decisions/{decisionId}/allocate` → הילד שולח הקצאה סופית שסכומה = הסכום שהתקבל
7. `GET /api/children/{childId}/balance` → יתרה מחושבת מה-Ledger
8. `POST /api/decisions/{decisionId}/reflection` → (אחרי שהוגדר תזכורת) תשובת רפלקציה

מסלול נוסף: `POST /api/chores` → `.../complete` → `.../approve` (יוצר MoneyEvent+Decision אוטומטית).
`POST /api/goals` → `POST /api/goals/{id}/contribute` (מושך מקופת Save הפתוחה).

## מה כבר בנוי

- Domain מלא: Household, Adult, Child, ChildAdult, Bucket, Ledger, LedgerEntry, MoneyEvent,
  Decision, DecisionAllocation, DecisionReflection, AllowancePlan, SavingGoal, Chore, Reward
- `DecisionEngine` + `LedgerCalculator` כשירותי Domain טהורים (עם טסטים)
- Vertical Slices: ReceiveAllowance, GetPendingDecision, AllocateMoney, GetChildBalance,
  CreateGoal, ContributeToGoal, CreateChore/CompleteChore/ApproveChore, CreateReward, SubmitReflection
- API מלא מעל כל זה + middleware שממיר `DomainException` ל-400 אחיד
- שלד Blazor Web App שמתקמפל ורץ (בלי מסכים אמיתיים עדיין)

## מה עדיין חסר (הצעדים הבאים, לפי סדר עדיפות טבעי)

1. **Auth אמיתי** - כרגע `OnboardingController` הוא bootstrap גס בלי JWT, בלי hash אמיתי לסיסמה
   (יש placeholder בקוד עם `TODO`). צריך: הרשמה, login להורה, PIN לילד, הרשאות Parent/Child.
2. **AllowancePlan scheduler** - Job שרץ תקופתית ומפעיל `ReceiveAllowanceHandler` אוטומטית
   לפי `Frequency`/`ScheduleDay` (יש Entity, אין עדיין ה-Job/Slice שמפעיל אותו).
3. **הסברי השלכות בזמן אמת** - יש כבר `DecisionEngine.ExplainSavingImpact` כפונקציית Domain,
   אבל היא עוד לא מחוברת ל-endpoint/UI שקורא לה תוך כדי שהילד גורר סליידר.
4. **Lesson, Achievement, Challenge, AI Coach** - מוזכרים באפיון אבל לא נבנו בכלל בסבב הזה -
   היקף גדול מספיק לסבב נפרד.
5. **מסכי Blazor אמיתיים** - במיוחד מסך ה-Decision Flow (הכי קריטי חווייתית) ו-Parent Dashboard.
6. **טסטים** - `KidsPocket.Application.Tests` ו-`KidsPocket.Api.Tests` הם שלד עם TODO בלבד.

## הערה טכנית
כל הקוד נכתב ידנית בלי גישה ל-dotnet SDK/אינטרנט בסביבה שלי (אין לי איך להריץ `dotnet build`
או NuGet restore כאן) - תריץ את זה אצלך ותגיד לי אם יש שגיאות קומפילציה, נתקן מהר.
# kidspoket
