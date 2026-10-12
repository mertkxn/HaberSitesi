using Microsoft.EntityFrameworkCore;
using SonGun.Models;

namespace SonGun.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Kategorileri Kontrol Et ve Ekle
        var categoryMap = new Dictionary<string, Category>();

        var defaultCategories = new List<Category>
        {
            new Category { Name = "Suikast", Description = "Tarihi ve siyasi figürlere yönelik planlı silahlı veya bombalı saldırılar sonucu gerçekleşen ölümler." },
            new Category { Name = "Zehirlenme", Description = "Kasıtlı veya şüpheli toksik maddeler, kimyasallar ve zehirler ile meydana gelen ölümler." },
            new Category { Name = "İntihar", Description = "Tarihe yön vermiş önemli şahsiyetlerin kendi hayatlarına son vermesi olayları." },
            new Category { Name = "Şüpheli Ölüm ve Kazalar", Description = "Gizemi tam aydınlatılamayan trajik kazalar ve arkasında soru işaretleri bırakan şüpheli vefatlar." },
            new Category { Name = "İdam ve Yargılama", Description = "Dönemin hukuki, siyasi veya devrim mahkemelerinin infaz kararlarıyla hayatı sonlananlar." },
            new Category { Name = "Hastalık ve Doğal Nedenler", Description = "Salgınlar, mesleki maruziyetler veya amansız hastalıklar nedeniyle yaşanan tarihi kayıplar." }
        };

        foreach (var defCat in defaultCategories)
        {
            var existing = await context.Categories.FirstOrDefaultAsync(c => c.Name == defCat.Name);
            if (existing == null)
            {
                context.Categories.Add(defCat);
                await context.SaveChangesAsync();
                categoryMap[defCat.Name] = defCat;
            }
            else
            {
                categoryMap[defCat.Name] = existing;
            }
        }

        // Eğer kategoriye bağlı olmayan haberler varsa ilk kategoriye bağla
        var existingNewsWithoutCategory = await context.News.Where(n => n.CategoryId == 0).ToListAsync();
        if (existingNewsWithoutCategory.Any())
        {
            var fallbackCategory = categoryMap.Values.First();
            foreach (var item in existingNewsWithoutCategory)
            {
                item.CategoryId = fallbackCategory.Id;
            }
            await context.SaveChangesAsync();
        }

        // 2. 20 Önemli Kişinin Gerçek Portreleriyle Haberleri
        var suikastId = categoryMap["Suikast"].Id;
        var zehirlenmeId = categoryMap["Zehirlenme"].Id;
        var intiharId = categoryMap["İntihar"].Id;
        var supheliKazaId = categoryMap["Şüpheli Ölüm ve Kazalar"].Id;
        var idamId = categoryMap["İdam ve Yargılama"].Id;
        var hastalikId = categoryMap["Hastalık ve Doğal Nedenler"].Id;

        var historicalNews = new List<News>
        {
            new News
            {
                Title = "Jül Sezar'ın Son Günü: Senatoda 23 Bıçak Darbesi (Ides of March)",
                CategoryId = suikastId,
                Author = "Azrail",
                ImageUrl = "/images/people/caesar.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                Content = @"M.Ö. 15 Mart 44'te Roma Diktatörü Jül Sezar, Pompey Tiyatrosu'nda toplanan Senato oturumuna katıldı. Senato binasına girmeden önce bir kahinin 'Mart ayının ortasından (Ides of March) sakın' uyarısını göz ardı etmişti.

Toplantı başlar başlamaz aralarında en güvendiği isimlerden Marcus Junius Brutus ve Gaius Cassius Longinus'un da bulunduğu yaklaşık 60 senatör Sezar'ın etrafını sardı. Togasının altına gizledikleri hançerleri çeken suikastçılar arka arkaya saldırdı. Sezar'ın son anlarında Brutus'u görünce söylediği rivayet edilen 'Sen de mi Brutus, oğlum?' sözleri tarihe kazındı. 

Yapılan otopside Sezar'ın bedeninde 23 bıçak darbesi tespit edildi ve bu darbelerden yalnızca birinin göğsünden aldığı ölümcül yara olduğu belirlendi. Sezar'ın katli, Roma Cumhuriyeti'ni kurtarmak yerine kanlı iç savaşlara ve nihayetinde Roma İmparatorluğu'nun kurulmasına yol açtı."
            },
            new News
            {
                Title = "Sokrates'in Baldıran Zehri ile İmtihanı: Düşüncenin Bedeli",
                CategoryId = zehirlenmeId,
                Author = "Azrail",
                ImageUrl = "/images/people/socrates.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-19),
                Content = @"Antik Atina'nın en büyük filozofu Sokrates, M.Ö. 399 yılında şehrin tanrılarına inanmamak ve gençlerin ahlakını bozmak suçlamasıyla 501 kişilik yurttaş mahkemesi önüne çıkarıldı.

Sokrates mahkemedeki ünlü savunmasında af dilemek ya da sürgünü kabul etmek yerine doğru bildiklerini savunmaya devam etti. Mahkemenin oy çokluğuyla verdiği idam kararının ardından zindanda infaz gününü bekledi. Dostu Kriton ve diğer öğrencilerinin hazırladığı kaçış planlarını 'yasalara ve devlete saygı' gerekçesiyle kesin bir dille reddetti.

İnfaz günü geldiğinde gardiyanın getirdiği bir kupa dolusu baldıran otu (conium) zehrini sükunetle içti. Bacaklarından kalbine doğru yayılan soğuklukla son nefesini verirken öğrencisi Kriton'a 'Asklepios'a bir horoz borcumuz var, ödemeyi unutma' diyerek felsefe tarihinin en unutulmaz vedasını gerçekleştirdi."
            },
            new News
            {
                Title = "Abraham Lincoln'e Ford Tiyatrosu'nda Hain Suikast",
                CategoryId = suikastId,
                Author = "Azrail",
                ImageUrl = "/images/people/lincoln.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-18),
                Content = @"14 Nisan 1865 Cuma akşamı, Amerikan İç Savaşı'nın sona ermesinden sadece beş gün sonra, ABD Başkanı Abraham Lincoln ve eşi Mary Todd Washington'daki Ford Tiyatrosu'nda 'Our American Cousin' adlı komedi oyununu izlemeye gittiler.

Oyunun en çok kahkaha atan sahnesinde, sesin silah sesini bastıracağını hesaplayan Konfederasyon sempatizanı ünlü aktör John Wilkes Booth, korumasız kalan başkanlık locasına gizlice sızdı. 44 kalibrelik Derringer tabancasıyla Lincoln'ü başının arkasından vurdu. Ardından sahneye atlayarak 'Sic semper tyrannis!' (Diktatörlerin sonu hep böyledir) diye bağırdı.

Ağır yaralanan Lincoln tiyatronun karşısındaki Petersen House'a taşındı. Ertesi sabah saat 07.22'de hayatını kaybeden Lincoln, köleliği kaldıran ve ulusu bir arada tutan başkan olarak tarihteki yerini aldı."
            },
            new News
            {
                Title = "Nil'in Son Kraliçesi Kleopatra'nın Gizemli Ölümü",
                CategoryId = intiharId,
                Author = "Azrail",
                ImageUrl = "/images/people/cleopatra.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-17),
                Content = @"Ptolemaios Hanedanı'nın son hükümdarı VII. Kleopatra, sevgilisi ve müttefiki Marcus Antonius'un Actium Deniz Muharebesi'ndeki yenilgisi ve intiharının ardından M.Ö. 30 yılının Ağustos ayında İskenderiye'deki sarayına kapandı.

Roma fatihi Octavianus'un (sonradan İmparator Augustus) kendisini esir alıp Roma sokaklarında zafer alayında zincirler içinde teşhir etmesine izin vermemekte kararlıydı. Kleopatra, hükümdarlık kaftanlarını giyerek ve kraliyet tacını takarak altın yatağına uzandı.

Rivayetlere göre, saraya gizlice sokulan bir incir sepetinin içine gizlenmiş zehirli Mısır kobrasının (asp) kendisini ve iki sadık hizmetkarını ısırmasını sağladı. Bazı tarihçiler ise kobra yerine zehirli bir merhem ya da karışım içtiğini savunmaktadır. Kleopatra'nın ölümüyle 3000 yıllık firavunlar dönemi ve Mısır Krallığı resmen sona erdi."
            },
            new News
            {
                Title = "Dallas Konvoyunda Açılan Ateş: John F. Kennedy'nin Trajik Sonu",
                CategoryId = suikastId,
                Author = "Azrail",
                ImageUrl = "/images/people/jfk.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-16),
                Content = @"22 Kasım 1963 Cuma günü yerel saatle 12.30'da ABD'nin 35. Başkanı John F. Kennedy, eşi Jacqueline Kennedy ile birlikte Teksas eyaletinin Dallas kentinde seçim kampanyası kapsamında üstü açık Lincoln Continental limuziniyle ilerliyordu.

Konvoy Dealey Plaza'ya girip Elm Caddesi'ne döndüğü sırada silah sesleri yankılandı. Teksas Okul Kitapları Deposu'nun 6. katından açılan ateş sonucu başkan boynundan ve başından vuruldu. Ağır yaralanan Kennedy acilen Parkland Memorial Hastanesi'ne kaldırıldı ancak doktorların tüm çabalarına rağmen yarım saat sonra öldüğü ilan edildi.

Olaydan kısa bir süre sonra tutuklanan eski deniz piyadesi Lee Harvey Oswald suikastçı olarak suçlandı, ancak iki gün sonra canlı yayında Jack Ruby tarafından vurularak öldürüldü. Kennedy suikastı, arkasındaki sayısız soru işareti ve komplo teorileriyle modern tarihin en gizemli cinayeti olmaya devam etmektedir."
            },
            new News
            {
                Title = "Paris Tünelinde Kara Gece: Prenses Diana'nın Trajik Kazası",
                CategoryId = supheliKazaId,
                Author = "Azrail",
                ImageUrl = "/images/people/diana.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                Content = @"31 Ağustos 1997 gecesi Galler Prensesi Diana, sevgilisi Dodi Al Fayed ile birlikte Paris'teki Ritz Oteli'nden ayrıldı. Kendilerini takip eden kalabalık paparazzi ordusunu atlatmak isteyen şoför Henri Paul, siyah Mercedes-Benz S280 aracıyla hızla uzaklaşmaya çalıştı.

Saat 00.23 sularında araç Pont de l'Alma tüneline girdiğinde kontrolünü kaybederek tünelin 13. beton sütununa yaklaşık 105 km hızla çarptı. Çarpmanın şiddetiyle Dodi Al Fayed ve şoför olay yerinde hayatını kaybetti.

Ağır yaralı olarak hurdaya dönen araçtan çıkarılan 'Halkın Prensesi' Diana, Pitié-Salpêtrière Hastanesi'ne nakledildi. Göğüs boşluğunda oluşan ağır iç kanama nedeniyle saat 04.00'te vefat etti. Olayın ardında paparazzilerin amansız takibi, aşırı hız ve emniyet kemeri takılmamış olmasının yanı sıra komplo teorileri de yıllarca tartışıldı."
            },
            new News
            {
                Title = "Dünyayı Fetheden Hükümdar: Büyük İskender'in Babil'deki Ani Vefatı",
                CategoryId = hastalikId,
                Author = "Azrail",
                ImageUrl = "/images/people/alexander.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                Content = @"Tuna Nehri'nden İndus Nehri'ne kadar uzanan bilinen dünyanın büyük kısmını fetheden Makedonya Kralı Büyük İskender, M.Ö. 323 yılının Haziran ayında Arabistan seferinin hazırlıkları için Babil Sarayı'nda bulunuyordu.

Komutanı Medius'un evinde verilen uzun bir ziyafetin ardından aniden şiddetli bir ateş ve karın ağrısı nöbetine yakalandı. Günler geçtikçe durumu kötüleşti; ateşi düşmedi ve konuşma yetisini kaybetti. Askerleri son bir kez krallarını görmek için yatağının önünden saygıyla geçtiğinde sadece gözleriyle selam verebiliyordu.

10 ya da 11 Haziran 323'te henüz 32 yaşındayken hayata veda etti. Kendisine imparatorluğunu kime bıraktığı sorulduğunda son fısıltısının 'En güçlü olana' olduğu söylenir. Ölüm sebebi sıtma, tifo, Batı Nil virüsü veya zehirlenme şüpheleriyle bugün halen tıp tarihçileri arasında tartışılmaktadır."
            },
            new News
            {
                Title = "Devrim Mahkemesi ve Giyotin: Kraliçe Marie Antoinette'in İdamı",
                CategoryId = idamId,
                Author = "Azrail",
                ImageUrl = "/images/people/marie_antoinette.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-13),
                Content = @"Fransız Devrimi'nin en radikal döneminde, Kral XVI. Louis'nin idamından dokuz ay sonra sıra eşi Marie Antoinette'e geldi. Aylarca Conciergerie zindanında zor şartlar altında tutulan eski Fransa Kraliçesi, Devrim Mahkemesi önüne çıkarıldı.

Vatana ihanet, devlet hazinesini tüketmek ve düşman güçlerle iş birliği yapmak suçlamalarıyla iki günlük yoğun yargılamanın ardından suçlu bulundu ve idama mahkum edildi. 16 Ekim 1793 sabahı beyaz sade bir elbise giydirilen kraliçe, elleri arkasından bağlanmış olarak açık bir at arabasında Devrim Meydanı'na (Place de la Révolution) götürüldü.

Giyotin basamaklarını çıkarken cellat Charles-Henri Sanson'un ayağına kazara bastığında 'Kusuruma bakmayın bayım, bilerek yapmadım' dedi. Saat 12.15'te giyotinin bıçağı indiğinde toplanan kalabalık 'Yaşasın Cumhuriyet!' çığlıkları attı."
            },
            new News
            {
                Title = "Şiddetsizliğin Mimarı Mahatma Gandi'ye Yönelen Kurşunlar",
                CategoryId = suikastId,
                Author = "Azrail",
                ImageUrl = "/images/people/gandhi.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                Content = @"Hindistan'ın bağımsızlığını pasif direniş ve şiddetsizlik (Ahimsa) felsefesiyle kazandıran 'Büyük Ruh' Mahatma Gandi, 30 Ocak 1948 akşamı saat 17.17'de Yeni Delhi'deki Birla Evi'nde her akşam düzenlediği dua toplantısına doğru yürüyordu.

Torunlarının omzuna yaslanarak yürüyen 78 yaşındaki liderin karşısına kalabalığın arasından çıkan aşırı milliyetçi Hindu Nathuram Godse çıktı. Önce saygıyla eğilen Godse, ardından Beretta marka yarı otomatik tabancasını çekerek çok yakın mesafeden Gandi'nin göğsüne ve karnına üç el ateş etti.

Gandi yere düşerken ellerini birleştirip 'Hey Ram!' (Ey Tanrım!) diyerek son nefesini verdi. Suikast, bağımsızlığını yeni kazanan ve dini çatışmalarla sarsılan Hindistan'da derin bir matem ve şok yarattı."
            },
            new News
            {
                Title = "Sanatın Yalnız Dehası: Vincent van Gogh'un Auvers Tarlalarındaki Vedası",
                CategoryId = intiharId,
                Author = "Azrail",
                ImageUrl = "/images/people/van_gogh.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-11),
                Content = @"Modern resim sanatının en büyük öncülerinden Vincent van Gogh, ömrü boyunca mücadele ettiği ağır psikolojik bunalımlar ve melankoli içinde yaşadı. Yaşamı boyunca yalnızca bir tek tablosunu satabilen sanatçı, 1890 yılının Mayıs ayında Fransa'nın Auvers-sur-Oise köyüne yerleşmişti.

27 Temmuz 1890 Pazar günü şövalesini ve boyalarını alıp buğday tarlalarına gitti. Akşama doğru kaldığı Ravoux Hanı'na döndüğünde yaralı olduğu fark edildi; tarlada küçük bir tabancayla kendisini göğsünden vurmuştu. Mermi kalbini ıskalamış ancak kaburga kemiğinde saplanıp kalmıştı.

Hemen çağrılan kardeşi Theo, başucuna koştu. İki gün boyunca yatağında piposunu tüttürerek kardeşiyle konuşan ressam, 29 Temmuz 1890 gecesi Theo'nun kollarında son nefesini verdi. Kardeşine söylediği son sözler ise 'La tristesse durera toujours' (Hüzün sonsuza dek sürecek) oldu."
            },
            new News
            {
                Title = "Çarlık Rusyası'nda Gizemli Bir Gece: Grigori Rasputin'in Zehirlenmesi ve Katli",
                CategoryId = zehirlenmeId,
                Author = "Azrail",
                ImageUrl = "/images/people/rasputin.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Content = @"Romanov Hanedanı üzerinde kurduğu esrarengiz nüfuzla Rus aristokrasisinin nefretini kazanan Sibiryalı 'çılgın keşiş' Grigori Rasputin, 30 Aralık 1916 gecesi Prens Feliks Yusupov tarafından Sankt-Petersburg'daki Yusupov Sarayı'na davet edildi.

Bodrum katındaki yemek odasında Rasputin'e ölümcül dozda potasyum siyanür enjekte edilmiş vişneli kekler ve şarap ikram edildi. Birkaç porsiyon tüketen Rasputin'in zehirden etkilenmediğini gören komplocular paniğe kapıldı. Bunun üzerine Prens Yusupov tabancasını çekerek Rasputin'i sırtından vurdu.

Öldü sanılan Rasputin aniden canlanarak Yusupov'un boğazına sarıldı ve bahçeye kaçtı. Dışarıda bekleyen diğer komplocular tarafından tekrar vurulan ve sopalarla darp edilen Rasputin, bir kilime sarılarak buz tutmuş Küçük Nevka Nehri'ne atıldı. Yapılan otopside ciğerlerinde su bulunması, nehre atıldığında dahi hayatta olduğunu ortaya koydu."
            },
            new News
            {
                Title = "Balkondaki Keskin Nişancı: Martin Luther King Jr.'ın Katledilmesi",
                CategoryId = suikastId,
                Author = "Azrail",
                ImageUrl = "/images/people/mlk.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-9),
                Content = @"'Bir Hayalim Var' konuşmasıyla ırk ayrımcılığına karşı küresel bir vicdan hareketi başlatan ve 1964 Nobel Barış Ödülü'nü kazanan Dr. Martin Luther King Jr., grevdeki temizlik işçilerine destek vermek amacıyla Tennessee eyaletinin Memphis kentine gitmişti.

4 Nisan 1968 Perşembe günü saat 18.01'de Lorraine Motel'in 306 numaralı odasının balkonuna çıkarak arkadaşlarıyla sohbet etmeye başladı. Karşıdaki pansiyonun penceresinden James Earl Ray tarafından Remington 7606 marka tüfekle ateşlenen tek bir mermi King'in sağ yanağından girip omuriliğini parçaladı.

Ağır yaralı olarak St. Joseph's Hastanesi'ne kaldırılan 39 yaşındaki sivil haklar lideri, saat 19.05'te hayatını kaybetti. Suikast haberinin yayılmasıyla birlikte Washington, Chicago ve Baltimore başta olmak üzere Amerika'nın dört bir yanında büyük sokak gösterileri ve isyanlar patlak verdi."
            },
            new News
            {
                Title = "Yüzyıl Savaşları'nın Bakiresi: Jeanne d'Arc'ın Yakılarak İdamı",
                CategoryId = idamId,
                Author = "Azrail",
                ImageUrl = "/images/people/joan_of_arc.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                Content = @"Yüzyıl Savaşları sırasında Tanrı'dan vahiy aldığını söyleyerek Fransız ordusunu İngiliz işgaline karşı tarihi zaferlere taşıyan 19 yaşındaki köylü kızı Jeanne d'Arc (Orléans Bakiresi), 1430'da Burgonyalılar tarafından esir alınarak İngilizlere satıldı.

Rouen kentinde İngiliz yanlısı Beauvais Piskoposu Pierre Cauchon başkanlığında kurulan engizisyon mahkemesinde aylarca sorgulandı. Büyücülük, kafirlik ve savaşta erkek kıyafeti giymek suçlamalarıyla suçlu bulunarak ölüm cezasına çarptırıldı.

30 Mayıs 1431 sabahı Rouen'in Eski Pazar Meydanı'nda yüksek bir odun yığınının üzerindeki direğe bağlandı. Ateş yakıldığında bir haç tutarak son nefesine kadar 'İsa, İsa!' diye haykırdı. Küllerinin kutsal emanet haline getirilmesini önlemek için kalıntıları Sen Nehri'ne döküldü; 1920 yılında ise Katolik Kilisesi tarafından azize ilan edildi."
            },
            new News
            {
                Title = "Yarım Kalan Requiem: Wolfgang Amadeus Mozart'ın Şüpheli Ölümü",
                CategoryId = supheliKazaId,
                Author = "Azrail",
                ImageUrl = "/images/people/mozart.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                Content = @"Müzik tarihinin tartışmasız en büyük dehalarından Wolfgang Amadeus Mozart, henüz 35 yaşındayken 5 Aralık 1791 gecesi saat 00.55'te Viyana'daki evinde aniden hayatını kaybetti.

Ömrünün son aylarında gizemli ve siyah giyimli bir yabancı tarafından sipariş edilen ölüler duası 'Requiem' bestesi üzerinde hummalı bir şekilde çalışıyordu. Mozart, eşi Constanze'a 'Biliyorum, bu Requiem'i kendim için besteliyorum, zehirlendiğimi hissediyorum' demişti. Hastalığının son haftalarında vücudunda şiddetli şişlikler, yüksek ateş ve kusma nöbetleri baş gösterdi.

Ölümünün ardından saray bestecisi Antonio Salieri'nin kıskançlık yüzünden Mozart'ı cıva ile zehirlediği dedikodusu tüm Avrupa'ya yayıldı. Modern tıp ise akut romatizmal ateş, trişinelloz veya böbrek yetmezliği ihtimallerini öne sürmektedir. Mozart'ın cenazesi parasızlık nedeniyle Aziz Marx Mezarlığı'nda toplu bir mezara defnedildi."
            },
            new News
            {
                Title = "Sürgündeki İmparatorun Sonu: Napolyon Bonapart ve Arsenik Esrarı",
                CategoryId = zehirlenmeId,
                Author = "Azrail",
                ImageUrl = "/images/people/napoleon.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                Content = @"Avrupa kıtasını fetheden ve hukuk devrimleri gerçekleştiren Fransız İmparatoru Napolyon Bonapart, 1815'teki Waterloo yenilgisinin ardından İngilizler tarafından Güney Atlantik'in ıssız Saint Helena adasına sürgüne gönderildi.

Longwood House adlı nemli ve rüzgarlı bir köşkte gözetim altında yaşayan Napolyon'un sağlığı 1821 başlarında hızla bozuldu. Karın ağrıları, mide bulantısı ve halsizlikten muzdarip olan devrik imparator, 5 Mayıs 1821 günü fırtınalı bir akşamda 'Fransa... Ordu... Ordunun başı... Joséphine...' diyerek vefat etti.

Resmi otopside ölüm sebebi babasınınki gibi mide kanseri olarak açıklandı. Ancak 1960'larda Napolyon'un saç tellerinde yapılan modern nükleer analizlerde ölümcül düzeyde arsenik tespit edildi. Bu bulgu, İngiliz muhafızlar veya saray maiyeti tarafından yavaş yavaş zehirlendiği iddialarını yeniden canlandırdı."
            },
            new News
            {
                Title = "Birinci Dünya Savaşı'nı Başlatan Kurşunlar: Franz Ferdinand Suikastı",
                CategoryId = suikastId,
                Author = "Azrail",
                ImageUrl = "/images/people/franz_ferdinand.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                Content = @"28 Haziran 1914 Pazar günü Avusturya-Macaristan İmparatorluğu veliahdı Arşidük Franz Ferdinand ve eşi Düşes Sophie, askeri tatbikatları denetlemek üzere Bosna'nın Saraybosna kentine geldiler.

Sabah saatlerinde konvoya bomba atılmış ancak veliahdın arabasını ıskalamıştı. Ziyareti iptal etmeyip yaralanan subayları hastanede ziyaret etmek isteyen arşidükün şoförü Franz Josef Caddesi'nde yanlış bir sokağa saptı. Arabanın geri manevra yapmaya çalıştığı o saniyelerde kaldırımda bulunan 'Kara El' örgütü mensubu 19 yaşındaki Gavrilo Princip fırsatı kaçırmadı.

Princip tabancasını çekerek yakın mesafeden iki el ateş etti; mermilerden biri Sophie'nin karnına, diğeri Ferdinand'ın boynuna isabet etti. Veliahdın eşine 'Sophie ölme, çocuklarımız için yaşa!' diye haykırışı fayda etmedi. Bu suikast, bir ay içinde dünya genelinde 20 milyondan fazla insanın hayatına mal olacak I. Dünya Savaşı'nı başlattı."
            },
            new News
            {
                Title = "Edebiyatın Yalnız Avcısı: Ernest Hemingway'in Idaho'daki Sessizliği",
                CategoryId = intiharId,
                Author = "Azrail",
                ImageUrl = "/images/people/hemingway.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                Content = @"'Çanlar Kimin İçin Çalıyor', 'Silahlara Veda' ve 'Yaşlı Adam ve Deniz' gibi başyapıtların yazarı, Nobel ve Pulitzer ödüllü Ernest Hemingway, yaşamı boyunca savaşlar, boğa güreşleri ve safari maceralarıyla maceracı bir profil çizmişti.

Ancak ömrünün son yıllarında şiddetli depresyon, hipertansiyon, karaciğer rahatsızlığı ve FBI tarafından takip edildiğine dair yoğun hezeyanlar yaşadı. Mayo Clinic'te geçirdiği elektroşok seansları hafızasını ve yazma yetisini köreltince derin bir çöküşe girdi.

2 Temmuz 1961 Pazar sabahı erkenden kalkan 61 yaşındaki Hemingway, Idaho'nun Ketchum kasabasındaki evinin kilerine indi. En sevdiği çift namlulu Vincenzo Bernardelli av tüfeğini aldı, giriş holüne geçti ve hayatına son verdi. Amerikan edebiyatının devi, ardında derin bir boşluk ve ölümsüz eserler bıraktı."
            },
            new News
            {
                Title = "Hollywood'un Işıltılı Trajedisi: Marilyn Monroe'nun Gizemli Ölümü",
                CategoryId = supheliKazaId,
                Author = "Azrail",
                ImageUrl = "/images/people/marilyn_monroe.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Content = @"20. yüzyılın en büyük sinema ikonu ve popüler kültür efsanesi Marilyn Monroe, 5 Ağustos 1962 Pazar günü sabahın erken saatlerinde Los Angeles'taki evinde hizmetçisi Eunice Murray tarafından yatağında ölü bulundu.

36 yaşındaki yıldızın yanında boş bir Nembutal (barbitürat) şişesi vardı ve elinde telefon ahizesi duruyordu. Toksikoloji raporunda kanında öldürücü dozda sakinleştirici ve uyku hapı tespit edildi. Los Angeles Adli Tıp Kurumu ölümü 'olası intihar' veya 'akut barbitürat zehirlenmesi' olarak nitelendirdi.

Ancak Monroe'nun o dönemde ABD Başkanı John F. Kennedy ve Adalet Bakanı Robert F. Kennedy ile olan yakın ilişkileri, evindeki günlük ve ses kayıtlarının kaybolması, CIA ve mafya şüpheleri ölümü etrafındaki sır perdesini hiçbir zaman tam olarak aralatmadı."
            },
            new News
            {
                Title = "İpatiev Evi'nin Bodrumunda Katliam: Romanov Ailesinin Kurşuna Dizilişi",
                CategoryId = idamId,
                Author = "Azrail",
                ImageUrl = "/images/people/nicholas_ii.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Content = @"1917 Bolşevik İhtilali'nin ardından tahttan feragat eden son Rus Çarı II. Nikolay ve ailesi, 1918 yazında Ural bölgesindeki Yekaterinburg kentine sürülerek tüccar İpatiev'in evinde sıkı gözetim altına alınmıştı.

Beyaz Ordu'nun kente yaklaşması üzerine Bolşevik yönetimi ailenin kurtarılmasını önlemek amacıyla infaz emri verdi. 17 Temmuz 1918 gecesi saat 01.30'da Çar, eşi Çariçe Aleksandra, kızları Olga, Tatyana, Mariya, Anastasya, veliaht Aleksey ve yanlarındaki dört sadık çalışan bodrum katına indirildi. Fotoğraf çekileceği bahanesiyle dizildikleri odada Komiser Yakov Yurovski infaz kararını okudu.

Hemen ardından odaya giren infaz müfrezesi tabancalarla ateş açtı. Elbiselerinin korselerine diktikleri elmaslar nedeniyle mermilerin sektiği genç grandüşesler süngülerle öldürüldü. Cenazeler ormana götürülerek asitle yakıldı ve gizli çukurlara gömüldü. Bu vahşi infaz, 304 yıllık Romanov Hanedanı'nın trajik sonu oldu."
            },
            new News
            {
                Title = "Bilim Uğruna Adanmış Bir Hayat: Marie Curie'nin Radyasyonla Gelen Sonu",
                CategoryId = hastalikId,
                Author = "Azrail",
                ImageUrl = "/images/people/marie_curie.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Content = @"Fizik ve kimya olmak üzere iki farklı bilim dalında Nobel Ödülü kazanan tarihteki tek insan olan Marie Curie (Madam Curie), radyoaktivite alanında yaptığı çığır açıcı keşiflerle modern bilimin seyrini değiştirdi.

Uranyum, polonyum ve radyum elementlerini ayrıştırırken radyasyonun insan sağlığı üzerindeki ölümcül etkileri henüz bilinmiyordu. Curie, onlarca yıl boyunca laboratuvar önlüğünün ceplerinde parıldayan radyoaktif tüpler taşıdı, parmak uçlarında radyasyon yanıklarıyla çalıştı ve hatta masasının çekmecesinde radyum tuzları sakladı.

Yıllar süren yüksek dozda radyasyon maruziyeti, kemik iliğini geri dönülmez şekilde tahrip etti. 4 Temmuz 1934'te Fransa'nın Passy kentindeki Sancellemoz Sanatoryumu'nda aplastik anemi nedeniyle 66 yaşında vefat etti. Bugün dahi Marie Curie'nin not defterleri, laboratuvar eşyaları ve hatta naaşı kurşun kaplı tabutta muhafaza edilecek düzeyde radyoaktiftir."
            }
        };

        foreach (var item in historicalNews)
        {
            var existing = await context.News.FirstOrDefaultAsync(n => n.Title == item.Title);
            if (existing == null)
            {
                context.News.Add(item);
            }
            else
            {
                existing.ImageUrl = item.ImageUrl;
                existing.CategoryId = item.CategoryId;
                existing.Author = "Azrail";
            }
        }

        var allNews = await context.News.ToListAsync();
        foreach (var n in allNews)
        {
            n.Author = "Azrail";
        }

        await context.SaveChangesAsync();
    }
}
