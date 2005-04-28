#define DE

namespace Drohne
{
    class i18n
    {
        #if DE
            public static string aboutWindow    = "Über Drohne";
            public static string buttonAbout    = "Über";
            public static string buttonClose    = "Schließen";
            public static string buttonPrev     = "Zurück";
            public static string buttonNext     = "Weiter";
            public static string dateFormat     = "HH:mm:ss   dd.MM.yyyy";
            public static string filterChecksum = "Prüfe Checksumme";
            public static string filterInvalid  = "Entferne ungültige Zeilen (default)";
            public static string filterIVCoord  = "Prüfe gültige Koordinaten (default)";
            public static string filterNullVal  = "Entferne Null Koordinaten (default)";
            public static string filterText
                = "\r\n    Drohne benutzt mehrere Funktionen, sogenannte Filter, um "
                + "RMC Logs zu bereinigen. Diese Filter können Sie in diesem Dialog "
                + "auswählen.\r\n"
                + "\r\n    Unerfahrenen Benutzer wird geraten die Default-Einstellung "
                + "der Filter zu benutzen, die für jede Konfiguration von GPS Empfänger "
                + "und deren Datenaufzeichnung die sicherste Methode zur Bereinigung der "
                + "Daten darstellt.\r\n"
                + "\r\n    Klicken Sie nun auf 'Weiter', um die Filter auf Ihre Daten "
                + "anzuwenden. Dieser Vorgang kann einige Sekunden in Anspruch nehmen.\r\n"
                + "\r\n    Es folgt eine ausführliche Beschreibung der Filter:\r\n"
                + "\r\n    1. " + filterInvalid + " - Prüft das Status-Feld in einer RMC"
                + "Zeile, besitzt es den Wert 'V', wird die Zeile entfernt.\r\n"
                + "\r\n    2. " + filterNullVal + " - Zeilen, deren Koordinaten gleich Null "
                + " oder 3600/12000 sind, werden entfernt.\r\n"
                + "\r\n    3. " + filterIVCoord + " - Diese Filter entfernt Zeilen mit "
                + "ungültigen Koordinaten, Längengrade größer als 90° und Breitengrade "
                + "größer als  180°.\r\n"
                + "\r\n    4. " + filterChecksum + " - Vergleicht die von Drohne berechnete "
                + "Checksumme mit der Checksumme der RMC Zeile. Stimmen beide Summen nicht "
                + "überein, wird die Zeile entfernt.";
            public static string labelBegin     = "Anfangszeitpunkt";
            public static string labelEnd       = "Endzeitpunkt";
            public static string pageAbout      = "Über";
            public static string pageLicense    = "Lizenzvereinbarung";
            public static string progDesc       = "Drohne - RMC Log Cleaner";    
            public static string progName       = "Drohne 0.1.0";
            public static string readClipboard  = "aus der Zwischenablage";
            public static string saveLog        = "RMC Daten speichern";
            public static string saveText
                = "\r\n    In diesem finalen Schritt können Sie nun ihre bereinigten GPS "
                + "Daten speichern. Die ListBox ermöglicht die Auswahl aller Touren in Ihren "
                + "GPS Daten. Ist die Listbox leer, so ist das ein Zeichen dafür, dass Ihre "
                + "Datenquelle keine RMC Zeilen enthält. Die beiden Eingabefelder Anfangszeitpunkt und "
                + "Endzeitpunkt erlauben ihnen ihre GPS Daten eines ausgewählten "
                + "Zeitraum zu speichern.\r\n"
                + "\r\n    Der Knopf 'Weiter' ruft den Dialog "
                + "zum Speichern ihre Daten auf. Nachdem Sie ihre Daten gespeichert haben, "
                + "können Sie das Programm beenden oder den Prozess für andere GPS Daten "
                + "wiederholen.";
            public static string selectFiles    = "Datei(en) auswählen";
            public static string sourceInfoClip = "Zwischenablage als Quelle gewählt";
            public static string sourceInfoFile = "Folgende Datei(en) als Quelle gewählt"; 
            public static string sourceInfoNone = "keine Quelle gewählt";
            public static string sourceNoClip   = "Zwischenablage leer";
            public static string sourceSelect   = "Sie müssen eine Quelle auswählen!";
            public static string sourceText     
                = "\r\n    Wählen Sie hier die Quelle ihrer GPS Daten. Benutzen "
                + "Sie 'Datei(en) auswählen', um eine oder mehrer Quelldateien "
                + "auszuwählen oder 'aus der Zwischenablage', um den Inhalt der "
                + "Zwischenablage als Quelle zu benutzen.\r\n"
                + "\r\n    Das Textfeld unter den beiden Knöpfen gibt Informationen "
                + "darüber wie Sie sich entscheiden haben.\r\n"
                + "\r\n    Wählen Sie eine Quelle und klicken Sie auf 'Weiter'.";
            public static string welcomeText     
                = "\r\n        Willkommen zu Drohne!\r\n"
                + "\r\n    Drohne ist ein kleines Programm, dass GPS Daten im NMEA "
                + "RMC Format oder Logdateien von GPS Empfängern, in denen RMC "
                + "Zeilen eingebettet sind, bereinigen kann.\r\n"
                + "\r\n    Drohne liest GPS Daten aus einer oder mehreren Dateien "
                + "oder aus der Zwischenablage und kann verschiedene Filter auf "
                + "RMC Zeilen anwenden, um ungültige GPS Daten oder Fehler bei der "
                + "Aufzeichnung zu bereinigen.\r\n"
                + "\r\n    Die bereinigten GPS Daten oder benutzerdefinierte "
                + "Zeiträume dieser können wieder in eine Datei geschrieben oder "
                + "in die Zwischenablage kopiert werden.\r\n"
                + "\r\n    Klicken Sie auf 'Weiter', um zu beginnen.";
        #elif EN
        #endif
    }
}

// vim:fileformat=dos
