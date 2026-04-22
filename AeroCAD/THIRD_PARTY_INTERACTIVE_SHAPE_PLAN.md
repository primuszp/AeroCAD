# AeroCAD Third-Party Interactive Shape Plan

## Cél

Az AeroCAD CAD engine-hez olyan bővítési pontot kell kialakítani, amelyen keresztül egy külső fejlesztő új interaktív rajzelemet tud hozzáadni úgy, hogy:

- ne kelljen a host belső controller logikáját másolnia
- a geometry külön tesztelhető maradjon
- a preview külön tesztelhető maradjon
- a létrehozott entity a közös CAD engine-en keresztül jöjjön létre
- a meglévő viselkedés ne törjön meg

## Nem cél

- nem vezetünk be általános plugin framework-öt mindenre
- nem írjuk át egyszerre az összes meglévő command controllert
- nem cseréljük le a jelenlegi UI vagy viewport réteget
- nem keverjük össze a shape logikát a dokumentummentéssel ebben a fázisban

## Jelenlegi kiindulópont

A `POLYGON` parancs most azért hasznos, mert megmutatja, hogyan lehet:

- interaktív inputot feldolgozni
- preview-t rajzolni
- CAD engine entity-t létrehozni
- végül mégis egy közös `Polyline` eredményt kapni

Ez jó alap, de jelenleg még erősen host-specifikus:

- közvetlenül ismeri a `Viewport` rubber objectet
- közvetlenül ismeri a layer / undo / document service-eket
- a step-flow kézzel van kódolva
- a preview logika több helyen van szétszórva

## Meglévő extension surface

A következő publikus regiszterek már léteznek, és az új shape contractnak ezekre kell épülnie:

- `ICadModuleCatalog` - modulok keresése név alapján
- `IEntityPluginCatalog` - entity pluginek keresése név vagy capability alapján
- `IInteractiveCommandRegistry` - interaktív parancsok keresése név alapján

A host már tud modulokat regisztrálni és assembly directory-ból betölteni.
Ez azt jelenti, hogy a third-party bővítésnek nem új plugin-rendszert kell kitalálnia,
hanem a meglévő registry rétegre kell tiszta shape contractot építenie.

### Jelenlegi extension típusok

| Mire van szükség | Mit kell implementálni |
|---|---|
| Új draw command, ami meglévő entityt hoz létre | `CommandControllerBase` + `InteractiveCommandRegistration` |
| Új entity type rendereléssel | `EntityPluginBase` leszármazott |
| Nem interaktív editor command | `EditorCommandDefinition` |
| Ezek csomagolása együtt | `CadModuleBase` leszármazott |

### Jelenlegi korlátok

Az új shape contract megalkotásánál ezekkel számolni kell:

- még nincs plugin-aware persistence
- még nincs entity extended property rendszer
- a command input jelenleg pont és scalar fókuszú
- a view-layer preview még részben host-specifikus

Ebből az következik, hogy a `POLYGON`-t nem teljesen új entitásként kell kezelni, hanem
olyan mintaként, amely a meglévő registry és module rendszert használja fel.

## Tervezett célarchitektúra

### 1. Shape Definition

A külső fejlesztő ne controller-t írjon elsődlegesen, hanem shape definíciót.

Feladat:

- leírni a lépéseket
- leírni a keywordöket
- leírni az alapértelmezett inputokat
- leírni, hogy melyik lépés után milyen input következik

### 2. Preview Factory

A preview ne a controllerben legyen kézzel összerakva.

Feladat:

- külön factory adja vissza a segédgeometriát
- külön factory adja vissza az entitásalak preview-t
- a polygon-szerű shape-eknél a referencia-kör, sugár és polygon külön kezelhető legyen

### 3. Commit Factory

A végső entity létrehozása is külön contract legyen.

Feladat:

- a shape kapja meg a szükséges pontokat és módokat
- a commit factory visszaadja a létrehozandó entity-t
- a host csak elhelyezi azt a megfelelő layeren

### 4. Registry

Az új shape-ek regisztrációja registry-n keresztül történjen.

Feladat:

- a host ismerje a regisztrált shape definíciókat
- külső assembly hozzá tudjon adni új shape-et
- discovery és query legyen tesztelve

### 5. Module packaging

A külső fejlesztő számára a végső belépési pont továbbra is egy module legyen.

Feladat:

- a shape definition legyen module-ban csomagolható
- a module tudjon commandot, entity plugint és shape registry entry-t is hordozni
- a host a meglévő catalogs alapján találja meg ezeket

Ez a réteg már most is ismerős a kódbázisban, ezért nem új framework-öt kell bevezetni,
csak tisztább contractot a meglévő surface fölé.

## Kiemelt követelmény: tesztvezérelt átvezetés

Minden lépést teszttel kell védeni.

Szabály:

1. előbb legyen teszt
2. utána refaktor vagy új contract
3. a régi viselkedés maradjon zöld
4. ha eltörik valami, a teszt azonnal mutassa

## Migrációs terv

### Fázis 1: A jelenlegi `POLYGON` viselkedés rögzítése

Mielőtt bármilyen refaktor történik, a mostani működés legyen kimondva tesztekben.

Tesztelendő:

- oldalszám default `<4>`
- üres input elfogadása
- `Center`
- `Inscribed`
- `Circumscribed`
- `Edge`
- `E` keyword
- preview színek és stílusok
- végső entity típusa: `Polyline`

Javasolt tesztfájlok:

- `PolygonCommandControllerTests`
- `RegularPolygonGeometryTests`
- `PolygonPreviewTests`

### Fázis 2: Geometry és preview szétválasztása

A shape geometria stateless, a preview külön factory.

Tesztelendő:

- center alapú polygon pontsor
- edge alapú polygon pontsor
- preview kör / polygon / sugár együtt
- preview ne függjön a rubber object belső állapotától

Javasolt tesztfájlok:

- `RegularPolygonGeometryTests`
- `PolygonPreviewFactoryTests`

### Fázis 3: Shape contract bevezetése

Meg kell jeleníteni a külső fejlesztőnek a minimális contractot.

Ezeket kell tesztelni:

- shape regisztráció
- lépéssorrend
- keyword feloldás
- default input
- commit eredmény

Javasolt tesztfájlok:

- `InteractiveShapeDefinitionTests`
- `InteractiveShapeRegistryTests`

### Fázis 4: `POLYGON` átállítása az új contractra

A `POLYGON` legyen az első shape, amely az új minta szerint fut.

Tesztelendő:

- a régi polygon tesztek továbbra is zöldek
- a command flow változatlanul működik
- a preview vizuális szabályai megmaradnak

### Fázis 5: Külső plugin minta

Kell egy valódi third-party példa.

Tesztelendő:

- plugin regisztrál shape-et
- host discovery megtalálja
- command fut
- entity létrejön

Javasolt tesztfájlok:

- `ExternalShapePluginTests`
- `PluginDiscoveryServiceTests`

### Fázis 5b: Extension surface szinkronizálása

A shape contract ne ellentmondjon a már létező registries-nek.

Tesztelendő:

- a shape regisztráció megjelenik a command registry-ben
- a module discovery megtalálja a shape-et hordozó modult
- az entity plugin capability-k nem sérülnek
- a host nem igényel új, párhuzamos discovery utat

### Fázis 6: További built-in controller-ek migrálása

Ha a minta stabil, akkor át lehet vezetni a többi interaktív shape-et.

Következő jelöltek:

- `Line`
- `Polyline`
- `Circle`
- `Arc`
- `Rectangle`

## Refaktor sorrend

1. `POLYGON` viselkedés rögzítése tesztekkel
2. preview és geometry szétválasztása
3. deklaratív shape contract bevezetése
4. `POLYGON` átállítása az új contractra
5. külső plugin minta elkészítése
6. további shape-ek migrálása

## Elvárt bővíthetőség

Egy külső fejlesztő a következőket tudja majd megtenni:

- új interaktív shape-et regisztrálni
- saját preview-t adni
- saját geometry buildert használni
- entity létrehozást a host engine-en keresztül elvégezni

## Elvárt védelem

Minden változtatás után legyen lefedve legalább egy:

- functional teszt
- regression teszt
- registry/discovery teszt
- geometry teszt

## Kimenet

Ennek a tervnek a végére az AeroCAD-nak legyen egy tiszta, third-party-barát interaktív shape bővítési mintája, amiből a `POLYGON` csak az első referencia implementáció.
