using System;

namespace Spoustec{
public class Napoveda{

public string nap = @"
Nápověda:

Horní lišta - tlačítka:
- Zkopírovat - zkopíruje výstup do schránky
- Přerušit - ukončí  program
- Počet opakování - vpravo (tolikrát se provede zadaný příkaz)

Lišta pro zadání příkazu:
- CTRL - otevře nabídku příkazů
- Cesta s mezerou nemusí být v ""

Průzkumník:
- Filtr - zobrazení jen některých přípon - např zadat: jpg mp3

Proměnné u otevřeného souboru:
§sl = cesta k souboru
§cn = cesta bez přípony
§c = celá cesta
§d = disk
§s = soubor
§p = přípona
§n = název souboru
Jméno souboru se použije ze zatrhlých položek nebo z vlastního seznamu.

Příkazy:
zprava text - vypíše text do okna
napis text - vypíše text na řádek, = echo, ale rychlejší
exit - ukončí program
cls - vymaže okno

Proměnné prostředí - možnost přidání používaných programů

Další příkazy - většina funguje z CMD

Další proměnné:
§cd = aktuální cesta
§i = počítadlo

např. zadat:
echo §i
napis §i

Windows 10 nepodporuje průhlednost okna.

Nastavení je uloženo tady:
%APPDATA%/Spoustec/

Autor programu: Martin Z.
";

}}
