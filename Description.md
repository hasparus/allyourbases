<div role="main"><span id="maincontent"></span><h2>Projekt</h2><div class="box generalbox center clearfix"><div class="no-overflow"><p><a name="yui_3_17_2_1_1494000456221_1383"></a><a name="yui_3_17_2_1_1494000456221_1384"></a><a name="yui_3_17_2_1_1494000456221_1385"></a>
<b>Projekty należy przygotować indywidualnie.</b></p><p><b>Q&amp;A na temat projektu:<a href="https://skos.ii.uni.wroc.pl/mod/page/view.php?id=3510"> link</a><br></b></p><h2><b>C</b><b>o będzie oceniane? </b>
</h2>
<p><b>Model konceptualny - </b>powinien składać się z diagramu E-R,
komentarza zawierającego więzy pominięte w diagramie, oraz opisu
ról wraz z funkcjonalnościami (zgodnie ze specyfikacją poniżej).</p>
<p><a name="yui_3_17_2_1_1493831227476_954"></a><b>Model fizyczny
- </b>powinien być plikiem sql nadającym się do czytania (i oceny)
przez człowieka. Powinien zawierać definicję wszystkich
niezbędnych użytkowników bazy i ich uprawnień, tabel, więzów,
indeksów, kluczy, akcji referencyjnych, funkcji, perspektyw i
wyzwalaczy. Nie jest niezbędne wykorzystanie wszystkich tych
udogodnień, ale tam, gdzie pasują, powinny być wykorzystywane.</p>
<p><b>Dokumentacja </b>projektu&nbsp; - ma się składać z pojedynczego
pliku pdf zawierającego ostateczny model konceptualny oraz dokładne
instrukcje, jak należy aplikację uruchomić. Dokumentacja ma
dotyczyć tego, co jest zaimplementowane; w szczególności, nie
można oddać samej dokumentacji, bez projektu.</p>
<p><b>Program - </b>kod źródłowy
oraz poprawność i efektywność działania.</p>
<p>Oddajemy archiwum zawierające wszystkie pliki źródłowe
programu, dokumentację w pliku pdf, model fizyczny w pliku sql oraz
polecenie typu make (ew. skrypt run.sh, itp.) umożliwiające
kompilację i uruchomienie systemu.</p>
<h1>Projekt: System wspomagający organizację
konferencji</h1><br>
<p><b>Uwaga: Pogrubioną czcionką - dodatkowe wyjaśnienia dodane po ogłoszeniu specyfikacji.</b><br></p><p>Jesteś odpowiedzialny za implementację systemu wspomagającego
organizację i przeprowadzenie konferencji. Twoim zadaniem jest
zaimplementowanie zdefiniowanego poniżej API. 
</p>
<p>System ma udostępniać API  niezależnym aplikacjom działającym
na urządzeniach mobilnych uczestników konferencji. Niemniej jednak,
ze względu na to, że interesuje nas tematyka baz danych przedmiotem
projektu jest stworzenie wersji deweloperskiej systemu, w której
wywołania API  będą wczytywane z dostarczonego pliku.</p>
<h4>Opis problemu 
</h4>
<p>Konferencja obejmuje od kilku do kilkunastu wydarzeń, z których
każde zawiera do kilkudziesięciu referatów. Każde wydarzenie
posiada datę rozpoczęcia i zakończenia, wydarzenia mogą się
pokrywać.</p>
<p><a name="yui_3_17_2_1_1493831227476_974"></a>Uczestnicy (kilkaset osób) rejestrują się na dowolnie wybrane przez siebie wydarzenia.
Każdy uczestnik może wygłaszać dowolną liczbę referatów (być
może 0). Ponadto uczestnicy mogą nawiązywać (symetryczne)
znajomości z innymi uczestnikami.</p>
<p> Uczestnicy korzystają z systemu poprzez aplikację na urządzeniu
mobilnym, która umożliwia przeglądanie danych konferencji, planu
wydarzeń dla danego uczestnika, a także proponowanie i
dowiadywanie się o dodatkowych referatach organizowanych
spontanicznie – uczestnik proponuje referat, organizator
konferencji go zatwierdza, przydziela salę oraz upublicznia.
Aplikacja rejestruje obecność uczestnika na poszczególnych
referatach, uczestnik może ocenić każdy z referatów
(niezależnie od tego czy referat się już odbył). Każdy uczestnik posiada unikalny login i hasło.</p>
<p>Organizator konferencji może definiować wydarzenia i ich
zawartość, przeglądać wszystkie zbierane dane, w tym również
rozmaite statystyki dotyczące aktywności uczestników. 
</p>
<h4>Technologie</h4>
<p>System Linux. Język programowania dowolny – wybór wymaga
zatwierdzenia przez prowadzącego  pracownię. Baza danych –
postgresql.</p>
<p>Twój program po uruchomieniu powinien przeczytać ze
standardowego wejścia ciąg wywołań funkcji API, a wyniki ich
działania wypisać na standardowe wyjście. 
</p>
<p>Wszystkie dane powinny być przechowywane w bazie danych,  efekt
działania każdej funkcji modyfikującej bazę, dla której wypisano
potwierdzenie wykonania (wartość OK) powinien być utrwalony.
Program może być uruchamiany wielokrotnie np. najpierw wczytanie
pliku z danymi początkowymi, a następnie kolejnych testów
poprawnościowych. Przy pierwszym uruchomieniu program powinien
utworzyć wszystkie niezbędne elementy bazy danych (tabele, więzy,
funkcje wyzwalacze) zgodnie z przygotowanym przez studenta modelem
fizycznym. Baza nie będzie  modyfikowana pomiędzy kolejnymi
uruchomieniami. Program nie będzie miał praw do<b> <strike>czytania,</strike></b> tworzenia
i zapisywania jakichkolwiek plików. <b>Program będzie mógł czytać pliki z bieżącego katalogu.</b><br></p>
<h4>
Format pliku wejściowego</h4>
<p>Każda linia pliku wejściowego zawiera obiekt JSON
(<a href="http://www.json.org/json-pl.html">http://www.json.org/json-pl.html</a>).
Każdy z obiektów opisuje wywołanie jednej funkcji API wraz z
argumentami.</p>
<p>Przykład: obiekt { "function": { "arg1":
"val1", "arg2": "val2" } }  oznacza
wywołanie funkcji o nazwie function z argumentem arg1 przyjmującym
wartość "val1" oraz arg2 – "val2".</p>
<p>W pierwszej linii wejścia znajduje się wywołanie funkcji open z
argumentami umożliwiającymi nawiązanie połączenia z bazą
danych.</p>
<h4>Przykładowe wejście</h4>
<pre>{ "open": { "baza": "stud", "login": "stud", "password": "d8578edf8458ce06fbc"}}
{ "function1": { "arg1": "value1", "arg2": "value2" } }
{ "function2": { "arg1": "value4", "arg2": "value3", "arg3": "value5" } }
{ "function3": { "arg1": "value6" } }
{ "function4": { } }</pre><h4>
Format wyjścia</h4>
<p>Dla każdego wywołania wypisz <b>w osobnej linii </b>obiekt JSON
zawierający status wykonania funkcji OK/ERROR/NOT IMPLEMENTED oraz
zwracane dane wg specyfikacji tej funkcji.</p>
<p>Format zwracanych danych (dla czytelności zawiera zakazane znaki
nowej linii):</p>
<pre><a name="s-1"></a><a name="s-3"></a><a name="s-4"></a><a name="s-5"></a><a name="s-6"></a><a name="s-7"></a><a name="s-8"></a><a name="s-9"></a><a name="s-10"></a><a name="s-11"></a><a name="s-12"></a><a name="s-13"></a><a name="s-14"></a><a name="s-15"></a><a name="s-16"></a><a name="s-19"></a><a name="s-20"></a><a name="s-21"></a><a name="s-22"></a><a name="s-23"></a><a name="s-24"></a><a name="s-25"></a><a name="s-26"></a><a name="s-27"></a><a name="s-28"></a>{ "status":"OK",<br>
  "data": [ { "atr1":"v1",<br>
              "atr2":"v2" },<br>
            { "atr1":"v3",<br>
              "atr2":"v3" } ]
}
</pre><p>
Tabela data zawiera wszystkie wynikowe krotki. Każda krotka zawiera
wartości dla wszystkich atrybutów.</p>
<h4>Przykładowe wyjście</h4>
<pre>{ "status": "OK" }
{ "status": "NOT IMPLEMENTED" }
{ "status": "OK", "data": [ { "a1": "v1", "a2": "v2"}, { "a1": "v3", "a2": "v3"} ] }
{ "status": "OK", "data": [ ] }
{ "status": "ERROR" }</pre><h4>
Format opisu API</h4>
<p> Oznaczenia: 
</p>
<p>O – wymaga autoryzacji jako organizator, U – wymaga
autoryzacji jako zwykły uczestnik, N – nie wymaga autoryzacji, * 
- wymagana na zaliczenie</p>
<p> &lt;function&gt; &lt;arg1&gt; &lt;arg2&gt; … &lt;argn&gt;  //
nazwa funkcji oraz nazwy jej argumentów</p>
<p>// opis działania funkcji 
</p>
<p>// opis  formatu wyniku: lista atrybutów wynikowych tabeli data</p>
<p>Aby uzyskać 10 punktów za część projekt (warunek
zaliczenia) należy oddać samodzielnie napisany program
implementujący wszystkie funkcje oznaczone znakiem *,  dokumentację
(zawierającą model konceptualny) oraz model fizyczny.</p>
<h4>Nawiązywanie połączenia i definiowanie danych
organizatora</h4>
<pre>(*) open &lt;baza&gt; &lt;login&gt; &lt;password&gt;
// przekazuje dane umożliwiające podłączenie Twojego programu do bazy - nazwę bazy, login oraz hasło, wywoływane dokładnie jeden raz, w pierwszej linii wejścia
// zwraca status OK/ERROR w zależności od tego czy udało się nawiązać połączenie z bazą 

(*) organizer &lt;secret&gt; &lt;newlogin&gt; &lt;newpassword&gt; <br>// tworzy uczestnika &lt;newlogin&gt; z uprawnieniami organizatora i hasłem &lt;newpassword&gt;, argument &lt;secret&gt; musi być równy d8578edf8458ce06fbc5bb76a58c5ca4 // zwraca status OK/ERROR 
</pre><h4>
Operacje modyfikujące bazę</h4>
<p>Każde z poniższych wywołań powinno zwrócić obiekt JSON
zawierający wyłącznie status wykonania:  OK/ERROR/NOT IMPLEMENTED.</p><br>
<pre>(*O) event &lt;login&gt; &lt;password&gt; &lt;eventname&gt; &lt;start_timestamp&gt; &lt;end_timestamp&gt; // rejestracja wydarzenia, napis &lt;eventname&gt; jest unikalny

(*O) user &lt;login&gt; &lt;password&gt; &lt;newlogin&gt; &lt;newpassword&gt; // rejestracja nowego uczestnika &lt;login&gt; i &lt;password&gt; służą do autoryzacji wywołującego funkcję, który musi posiadać uprawnienia organizatora, &lt;newlogin&gt; &lt;newpassword&gt; są danymi nowego uczestnika, &lt;newlogin&gt; jest unikalny

(*O) talk &lt;login&gt; &lt;password&gt; &lt;speakerlogin&gt; &lt;talk&gt; &lt;title&gt; &lt;start_timestamp&gt; &lt;room&gt; &lt;initial_evaluation&gt; &lt;eventname&gt; // rejestracja referatu/zatwierdzenie referatu spontanicznego, &lt;talk&gt; jest unikalnym identyfikatorem referatu, &lt;initial_evaluation&gt; jest oceną organizatora w skali 0-10 – jest to ocena traktowana tak samo <b>jak ocena uczestnika obecnego na referacie</b>, &lt;eventname&gt; <b>jest nazwą wydarzenia, którego częścią jest dany referat -</b> może być pustym napisem, co oznacza, że referat nie jest przydzielony do jakiegokolwiek wydarzenia

(*U) register_user_for_event &lt;login&gt; &lt;password&gt; &lt;eventname&gt; // rejestracja uczestnika &lt;login&gt; na wydarzenie &lt;eventname&gt;

(*U) attendance &lt;login&gt; &lt;password&gt; &lt;talk&gt; // odnotowanie faktycznej obecności uczestnika &lt;login&gt; na referacie &lt;talk&gt;

(*U) evaluation &lt;login&gt; &lt;password&gt; &lt;talk&gt; &lt;rating&gt; // ocena referatu &lt;talk&gt; w skali 0-10 przez uczestnika &lt;login&gt;

(O) reject &lt;login&gt; &lt;password&gt; &lt;talk&gt; // usuwa referat spontaniczny &lt;talk&gt; z listy zaproponowanych,

(U) proposal  &lt;login&gt; &lt;password&gt; &lt;talk&gt; &lt;title&gt; &lt;start_timestamp&gt; // propozycja referatu spontanicznego, &lt;talk&gt; - unikalny identyfikator referatu

(U) friends &lt;login1&gt; &lt;password&gt; &lt;login2&gt; // uczestnik &lt;login1&gt; chce nawiązać znajomość z uczestnikiem &lt;login2&gt;, znajomość uznajemy za nawiązaną jeśli obaj uczestnicy chcą ją nawiązać tj. po wywołaniach friends &lt;login1&gt; &lt;password1&gt; &lt;login2&gt; i friends &lt;login2&gt; &lt;password2&gt; &lt;login1&gt;

</pre><h4>Pozostałe operacje</h4>
<p>Każde z poniższych wywołań powinno zwrócić obiekt JSON
zawierający status wykonania OK/ERROR, a także tabelę data
zawierającą krotki wartości atrybutów wg specyfikacji poniżej.</p>
<p><br>
<br>

</p>
<pre>(*N) user_plan &lt;login&gt; &lt;limit&gt; // zwraca plan najbliższych referatów z wydarzeń, na które dany uczestnik jest zapisany (wg rejestracji na wydarzenia) posortowany wg czasu rozpoczęcia, wypisuje pierwsze &lt;limit&gt; referatów, przy czym 0 oznacza, że należy wypisać wszystkie
// Atrybuty zwracanych krotek: 
   &lt;login&gt; &lt;talk&gt; &lt;start_timestamp&gt; &lt;title&gt; &lt;room&gt;

(*N) day_plan &lt;timestamp&gt; // zwraca listę wszystkich referatów zaplanowanych na dany dzień posortowaną rosnąco wg sal, w drugiej kolejności wg czasu rozpoczęcia
//  &lt;talk&gt; &lt;start_timestamp&gt; &lt;title&gt; &lt;room&gt;

(*N) best_talks &lt;start_timestamp&gt; &lt;end_timestamp&gt; &lt;limit&gt; &lt;all&gt; // zwraca referaty rozpoczynające się w  danym przedziale czasowym posortowane malejąco wg średniej oceny uczestników, przy czym jeśli &lt;all&gt; jest równe 1 należy wziąć pod uwagę wszystkie oceny, w przeciwnym przypadku tylko oceny uczestników, którzy byli na referacie obecni, wypisuje pierwsze &lt;limit&gt; referatów, przy czym 0 oznacza, że należy wypisać wszystkie
//  &lt;talk&gt; &lt;start_timestamp&gt; &lt;title&gt; &lt;room&gt;

(*N) most_popular_talks &lt;start_timestamp&gt; &lt;end_timestamp&gt; &lt;limit&gt; // zwraca referaty rozpoczynające się w podanym przedziału czasowego posortowane malejąco wg obecności, wypisuje pierwsze &lt;limit&gt; referatów, przy czym 0 oznacza, że należy wypisać wszystkie
//  &lt;talk&gt; &lt;start_timestamp&gt; &lt;title&gt; &lt;room&gt;

(*U) attended_talks &lt;login&gt; &lt;password&gt; // zwraca dla danego uczestnika referaty, na których był obecny 
//  &lt;talk&gt; &lt;start_timestamp&gt; &lt;title&gt; &lt;room&gt;

(*O) abandoned_talks &lt;login&gt; &lt;password&gt;  &lt;limit&gt; // zwraca listę referatów posortowaną malejąco wg liczby uczestników &lt;number&gt; zarejestrowanych na wydarzenie obejmujące referat, którzy nie byli na tym referacie obecni, wypisuje pierwsze &lt;limit&gt; referatów, przy czym 0 oznacza, że należy wypisać wszystkie
//  &lt;talk&gt; &lt;start_timestamp&gt; &lt;title&gt; &lt;room&gt; &lt;number&gt;

(N) recently_added_talks &lt;limit&gt; // zwraca listę ostatnio zarejestrowanych referatów, wypisuje ostatnie &lt;limit&gt; referatów wg daty zarejestrowania, przy czym 0 oznacza, że należy wypisać wszystkie
//  &lt;talk&gt; &lt;speakerlogin&gt; &lt;start_timestamp&gt; &lt;title&gt; &lt;room&gt;

(U/O) rejected_talks &lt;login&gt; &lt;password&gt; // jeśli wywołujący ma uprawnienia organizatora zwraca listę wszystkich odrzuconych referatów spontanicznych, w przeciwnym przypadku listę odrzuconych referatów wywołującego ją uczestnika 
//  &lt;talk&gt; &lt;speakerlogin&gt; &lt;start_timestamp&gt; &lt;title&gt;

(O) proposals &lt;login&gt; &lt;password&gt; // zwraca listę propozycji referatów spontanicznych do zatwierdzenia lub odrzucenia, zatwierdzenie lub odrzucenie referatu polega na wywołaniu przez organizatora funkcji talk lub reject z odpowiednimi parametrami
//  &lt;talk&gt; &lt;speakerlogin&gt; &lt;start_timestamp&gt; &lt;title&gt;

(U) friends_talks &lt;login&gt; &lt;password&gt; &lt;start_timestamp&gt; &lt;end_timestamp&gt; &lt;limit&gt; // lista referatów  rozpoczynających się w podanym przedziale czasowym wygłaszanych przez znajomych danego uczestnika posortowana wg czasu rozpoczęcia, wypisuje pierwsze &lt;limit&gt; referatów, przy czym 0 oznacza, że należy wypisać wszystkie
//  &lt;talk&gt; &lt;speakerlogin&gt; &lt;start_timestamp&gt; &lt;title&gt; &lt;room&gt;

(U) friends_events &lt;login&gt; &lt;password&gt; &lt;event&gt; // lista znajomych uczestniczących w danym wydarzeniu
//  &lt;login&gt; &lt;event&gt; &lt;friendlogin&gt; 

(U) recommended_talks &lt;login&gt; &lt;password&gt; &lt;start_timestamp&gt; &lt;end_timestamp&gt; &lt;limit&gt; // zwraca referaty rozpoczynające się w podanym przedziale czasowym, które mogą zainteresować danego uczestnika (zaproponuj parametr &lt;score&gt; obliczany na podstawie dostępnych danych – ocen, obecności, znajomości itp.), wypisuje pierwsze &lt;limit&gt; referatów wg nalepszego &lt;score&gt;, przy czym 0 oznacza, że należy wypisać wszystkie
//  &lt;talk&gt; &lt;speakerlogin&gt; &lt;start_timestamp&gt; &lt;title&gt; &lt;room&gt; &lt;score&gt;
</pre><br>Q&amp;A na temat projektu:<a href="https://skos.ii.uni.wroc.pl/mod/page/view.php?id=3510"> link</a><br><div><br></div></div></div><div class="modified">Ostatnia modyfikacja: wtorek, 23 maj 2017, 22:57 </div></div>