# SkiSimulator - C# wpf applikation & databas som hanterar skidåkare.

## Översikt
SkiSimulator är en klass som används för att simulera skidsäsonger eller skiddagar.
Den skapar slumpmässiga skiddagar och skidåkningar per åkare och hanterar olika faktorer som säsonger, liftval och åktider.

## Repositories
Dbrepository hanterar metoder som hämtar och matar in data i databasen.
DbExceptions hanterar felmeddelanden utöver de som applikationen hanterar

# Models
Klasserna som motsvarar tabellerna i databasen

## Viktig information innan ni kör applikationen

Innan applikationen körs måste databasen återskapas via backupen som ligger i repot. 
Det är en PostgreSQL databas skapad i PGadmin.

💡 Tips: Se till att databasanslutningen är korrekt konfigurerad innan ni startar applikationen.

