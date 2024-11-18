<div align="center">

# Dokument o dizajne hry
</div>

### Názov hry:
&emsp;Souls of Nav

### Hlavná myšlienka:
&emsp;Akčné dobrodružné RPG so zážitkom pre viacerých hráčov

### Koncept:
&emsp;<b>Žáner:</b> RPG <br>
&emsp;<b>Cieľové publikum:</b> 15+ <br>
&emsp;<b>Platforma:</b> PC <br>

## Úvod:
<p>
<b>Souls of Nav</b>
 je akčná dobrodružná hra vo svete založená na slovanskej mytológii. Je vojna. Hráč prevezme kontrolu nad bezmenným vojakom, ktorý zomrel počas tejto vojny, po ktorej opäť nadobudol vedomosť a ocitol sa v Nav (slovanský posmrtný život). Po nejakom čase sa presvedčí, že niečo nie je v poriadku...
</p>

### Dôvody pre výber žánru RPG:
<ol>
 <li>
 <b>Osobné záujmy a skúsenosti</b>:
 Tento žáner som si vybral pre svoje vlastné preferencie v hrách. Poznám veľa RPG a hral som ich pomerne málo. Myslím, že mi to dáva veľa skúseností s tým, ako by RPG malo a nemalo vyzerať.
 </li> <p>
 <li>
 <b>Výhody vývoja</b>:
 RPG môžu obsahovať mnoho rôznych mechaník a funkcií. To mi dáva veľa priestoru na rozšírenie.
 </li>
</ol>

### Vývojové prostredie:
<p>
<b>Unity Engine:</b>
 Tento engine som si vybral pre množstvo funkcií, ktoré poskytuje pre 2D a 3D vývoj. Používa tiež C#, ktorý poznám. Je dobre zdokumentovaný a podporovaný veľkou komunitou vývojárov.
</p>

## ciele
### Hlavný cieľ:
<p>
 Hlavným cieľom bude implementácia počítačovej hry žánru RPG s možnosťou hrania pre viacerých hráčov pomocou Unity. Výstupom projektu bude kompletná funkčná počítačová hra primeranej obtiažnosti s herným časom približne 40 minút. Podstatnými vlastnosťami hry bude vlastná grafika, rôznorodosť herných mechanizmov a nutnosť spolupráce s ostatnými.
</p>

### Čiastkové ciele:
<ol>
    <li>
    Vytvoriť a použiť primeranú grafiku tvoriacu vizuálny celok vrátane menu a užívateľského rozhrania.
    <ul>
        <li>pasivny ciel</li>
    </ul>
    </li><li>
    Vytvoriť postavu hráča s pokročilým bojovým systémom, inventárom.
    <ul>
        <li>aktivny ciel</li>
        <li>dlhodobby proces</li>
        <li>hotove:
            <ul>
                <li>inventar (40%)</li>
                <li>equipment (20%)</li>
                <li>bojovy system (10%)</li>
            </ul>
        </li>
    </ul>
    </li><li>
    Vytvoriť 5 typov bežných nepriateľov z pohľadu funkčnosti, každý v 2 variantoch.
    <ul>
        <li>aktivny ciel</li>
        <li>dlhodobby proces</li>
        <li>hotove:
            <ul>
                <li>-</li>
            </ul>
        </li>
    </ul>
    </li><li>
    Vytvoriť jedného finálneho bossa, s rôznymi typmi útokov meniacich sa v podľa počtu životov bossa.
    <ul>
        <li>aktivny ciel</li>
        <li>kratkodoby proces</li>
        <li>hotove:
            <ul>
                <li>-</li>
            </ul>
        </li>
    </ul>
    </li><li>
    Vytvoriť bojovú inteligenciu nepriateľov, ktorá bude riadiť ich správanie a útoky.
    <ul>
        <li>aktivny ciel</li>
        <li>dlhodobby proces</li>
        <li>spojeny s c. cielom 3</li>
        <li>hotove:
            <ul>
                <li>-</li>
            </ul>
        </li>
    </ul>
    </li><li>
    Vytvoriť systém koristi, kde z porazených nepriateľov hráč získava predmety, použiteľné ako aj nositeľné predmety.
    <ul>
        <li>aktivny ciel</li>
        <li>dlhodobby proces</li>
        <li>spojeny s c. cielom 3, 4, 5</li>
        <li>hotove:
            <ul>
                <li>-</li>
            </ul>
        </li>
    </ul>
    </li><li>
    Vytvoriť “strom schopností” kde hráč môže investovať získané body schopností na získanie nových schopností a permanentné vylepšenie svojich vlastností.
    <ul>
        <li>aktivny ciel</li>
        <li>dlhodobby proces</li>
        <li>hotove:
            <ul>
                <li>-</li>
            </ul>
        </li>
    </ul>
    </li><li>
    Použiť pokročilejšie princípy programovania pri vytváraní funkcionalít hry.
    <ul>
        <li>pasivny ciel</li>
    </ul>
    </li><li>
    Vložiť vhodnú hudbu do pozadia a ozvučiť všetky akcie hráča a nepriateľov.
    <ul>
        <li>aktivny ciel</li>
        <li>jednorazovy proces</li>
    </ul>
    </li>
</ol>

## Hrateľnosť
<p>
 <b>Základný princíp hry</b>: Zabíjajte nepriateľov, posilňujte sa, odomykajte drsnejšie oblasti. Spolupracujte s ostatnými hráčmi. Nakoniec sa dostaňte ku konečnému Bossovi a porazte ho.
</p>

### Herný svet
<p>
 Nav je reprezentácia slovanského posmrtného života, pokrytá pláňami trávy. Na západnej strane Nav je Sea of ​​Living, ktoré oddeľuje vchod do Mortal Realm. Na východe je Valesov palác a pláne sužované korupciou.
</p>

### Mechanika
<ul>
 <li>
 <b>Systém inventára</b>: obsahuje toľko vecí, koľko hráč momentálne môže mať
 </li> <li>
 <b>Bojové štýly</b>: rôzne druhy zbraní majú rôzne použitie.
 </li> <li>
 <b>Rôzne typy poškodenia</b>: každý typ poškodenia má iné využitie proti nepriateľovi - postavy a vybavenie majú rôznu odolnosť voči jednotlivým typom poškodenia
 </li> <li>
 <b>Umelá inteligencia nepriateľa</b>: útočné / obranné správanie.
 </li>
 <!--<b>Použitie mágie</b>: -->
</ul>
<!--### Znaky-->V