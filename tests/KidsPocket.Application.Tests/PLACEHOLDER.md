# TODO
כתוב כאן טסטים ל-Handlers עם KidsPocketDbContext מבוסס InMemory provider.
דוגמה טובה להתחיל ממנה: ReceiveAllowanceHandler -> בודק שנוצר MoneyEvent + Decision במצב Pending.
AllocateMoneyHandler -> בודק שסכום שלא תואם ל-MoneyEvent.Amount זורק DomainException,
ושכשההקצאה תקינה נוצרות LedgerEntries נכונות ושהיתרה המחושבת ב-GetChildBalanceHandler תואמת.
