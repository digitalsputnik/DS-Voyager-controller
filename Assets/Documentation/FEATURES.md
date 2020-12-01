## In this file is list of features that we would like to implement.

### Kaspar
 1. Video compression - capacity to use any image or video filmed by the same handheld device
 2. Animation control - preliminary timeline implentation
 3. DMX controller software - sACN/ArtNet input to Voyager stream
 4. Dropbox/Gdrive integration for project sharing
 5. Voyaberry (ESP32/Raspberry based) - to control ARRI and other lamps used in industry.
 6. ESP32 Voyager implementation - first to integrate DS lights into Voyager infrastructure
 7. Monet release/multicontroller support (playback + ITSHX)
 8. Matrix mapping
 9. Underwater usage (playback + ITSHX)
 10. Project and video management (autosave)
 11. Voyager Controller UX
---
### Taaniel
5. Device to device project sharing
12. NDI integration to Voyager Controller for virtual studios

### Taavet

Seda listi tehes mõtlesin sellele, et kui Roadmap oleks üleval kodulehel, siis mis mind huvitaks. Pea-aegu, et iga punkt hargneb muidugi väiksemateks tükkideks lahti ja vajab palju tööd. Ma olen rääkinud seda varem ja arvan ka siiani, et meil on omajagu fundamentaalseid asju (pean silmas punkte 3, 4, 5 ja 6), mis tuleks korda teha, et tulevik oleks helgem ja kindlam. Kui baas asjad on korras, siis selle peale on uusi feature'eid kindlam kirjutada.

1. **Video compression** - See on juba tegemisel, tuleks lõpetada.
2. **DMX controller software** - Teha lihtsa UI'ga valmis, kasutades DigitalSputnik.Net libra - selle taga oli raha ka.
3. **Communication protocol korda** - Läbi mõelda, kõik kirja panna ja siis implementeerida mõlemalt poolt. Unustada ka JSON ja võtta kasutusele ka mõni Reliable UDP lahendus. Kas teha ise või kasutada olemasolevat lahendust (nt. https://github.com/RevenantX/LiteNetLib).
4. **DigitalSputnik.Net** - Vaadata libra üle, teha kõik korda ja avalikustada, et kaasata teisi arendajaid ja asjahuvilisi. Leian, et mida varem, seda parem.
5. **Embedded render engine** - Lambi poolne soft kirjutada süsteemi keelega (nt. C, C++, Rust), et oleks suurem kontroll masina üle ja et koodibaasi saaks kasutada ka uuematel microcontrolleritel. 
6. **Voyager Controller UX ja Monet UX** - Need kaks eraldi programmi võiksid kasutada samasuguseid loogikaid ja UI'd aga sihtides erinevaid kasutajaid. Tuleks vaadata hoolega üle, millised feature'id lähevad ühele, millised teisele, või hoopis mõlemale. UX peaks läbima ühte ja samasugust flow'd mõlemas rakenduses võttes käiku ka tavad mis esinevad teistes rakendustes ja mis tunduvad klientidele intuitiivsena. Siinkohal ootan ka Jan'i arvamust, kes UI'd ette heitis - küllap tal on palju häid ideid kuidas asi paremaks teha.
7. **Multi-controller support** - Pärast UI paitamist saaks selle juurde minna.
8. **Animation control** - Esialgu Lambi ja Libra vahel teha, pärast seda mõelda, kuidas UI'se implementeerida. 
9. **Bluetooth library** - Seda sooviti panna store ülesse.


### Jan
1 ... 3  
3.5 Underwater usage (playback + ITSHX)   
4 Voyager libra + Project and video management (autosave)  
...  
9 Voyager Controller UX  
10 Dropbox/Gdrive integration for project sharing  


### Joosep
1. Video compression - capacity to use any image or video filmed by the same handheld device
2. DMX controller software - sACN/ArtNet input to Voyager stream
3. DigitalSputnik.Net + Communication Protocol
4. Voyaberry (ESP32/Raspberry based) - to control ARRI and other lamps used in industry.
5. ESP32 Voyager implementation - first to integrate DS lights into Voyager infrastructure
6. Monet release/multicontroller support (playback + ITSHX)
7. Voyager Controller UI/UX
8. Animation control - preliminary timeline implentation
9. Project and video management (autosave)
10. Dropbox/Gdrive integration for project sharing
11. Underwater usage (playback + ITSHX)
12. Matrix mapping
13. Bluetooth library
14. Smart home integration


### Mari
- TimeLine: video management and ordering
- UI: design + wording for Messages/Alerts/Errors etc.
- Project management and fast share + merge: import/export lamp (with all settings)
- Connection stability and distance + difficult conditions: wire control
- FX settings and CW Sliders: value range vs outcome proportions + for ex Possibility for lower Intesity than 1%
- Easy music/rhythm sync: stream from some other app? (in addition to ex: After Effects) - or some Play/Pause/Stop sync possibility..
- Voice control (preset and/or custom triggers or other smart home solutions.. + Date/Time control..)



