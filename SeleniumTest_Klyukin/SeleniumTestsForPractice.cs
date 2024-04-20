using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FluentAssertions;

namespace SeleniumTest_Klyukin;

public class SeleniumTestsForPractice
{
    public ChromeDriver driver;
    
    // 1. Тест на авторизацию
    [Test]
    public void AuthorizationTest()
    {
        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--start-maximized", "--disable-extensions");
       
        // Входим в Хром
        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
       
        // Переходим по урл https://staff-testing.testkontur.ru
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Username")));
        
        // Вводим логин и пароль
        var login = driver.FindElement(By.Id("Username"));
        login.SendKeys("a.o.klyukin@yandex.ru");
        var password = driver.FindElement(By.Name("Password"));
        password.SendKeys("IaMrEaDytony35!");
        
        // Нажимаем на кнопку "Войти"
        var enter = driver.FindElement(By.Name("button"));
        enter.Click();
        var news = driver.FindElement(By.CssSelector("[data-tid='Feed']"));
        
        // Проверяем, что мы находимся на нужной странице
        var currentUrl = driver.Url;
        currentUrl.Should().Be("https://staff-testing.testkontur.ru/news");

    }

    // 2. Тест перехода на страницу Сообщества через боковое меню (при полноэкранном запуске)
    [Test]
    public void NavigationTest()
    {
        // Авторизация
        Authorization();
        
        // Кликаем на "Сообщества"
        var community = driver.FindElements(By.CssSelector("[data-tid='Community']")).First(element => element.Displayed);
        community.Click();
        
        // Проверяем, что Сообщества есть на странице
        var communityTitle = driver.FindElement(By.CssSelector("[data-tid='Title']"));
        communityTitle.Should().NotBeNull();
        
    }
    
    // 3. Тест на наличие поисковой строки в окне поиска по файлам
    [Test] public void FilesSearchPageTest()
    {
        // Авторизация
        Authorization();
        
        // Переходим на страницу Файлы
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/files");
        
        // Кликаем на кнопку поиска
        var searchButton = driver.FindElement(By.CssSelector("[data-tid='Search']"));
        searchButton.Click();
     
        // Проверяем, что в открывшемся окне есть поисковая строка
        var filesSearchBar = driver.FindElement(By.CssSelector("[placeholder='Введите название файла или папки']"));
        filesSearchBar.Should().NotBeNull();

    }

    // 4. Тест на создание сообщества
    [Test]
    public void AddingCommunityTest()
    {
        // Авторизация
        Authorization();
        
        // Переходим на страницу Сообщества
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/communities");
        
        //Нажимаем кнопку Создать
        var addCommunityButton = driver.FindElement(By.CssSelector("[class='sc-juXuNZ sc-ecQkzk WTxfS vPeNx']")); // больше не за что было зацепиться
        addCommunityButton.Click();
        
        // Генерируем уникальное название для сообщества, выводим его
        var communityName = Guid.NewGuid().ToString();
        Console.WriteLine("Уникальное название сообщества: " + communityName);
        

        // Находим поле Название сообщества, вводим туда сгенерированное выше название
        var communityNameBar = driver.FindElement(By.CssSelector("[placeholder='Название сообщества']"));
        communityNameBar.SendKeys(communityName);
        
        // Нажимаем на кнопку Создать
        var createButton = driver.FindElement(By.CssSelector("[class='react-ui-m0adju']")); // больше не за что было зацепиться
        createButton.Click();
        
        // Переходим на страницу созданного сообщества, закрывая форму редактирования
        var closeCommunitySettings = driver.FindElement(By.CssSelector("[class='sc-juXuNZ kVHSha']")); // больше не за что было зацепиться
        closeCommunitySettings.Click();
        
        // Сравниваем название в заголовке созданного сообщества со сгенерированным значением
        var displayedCommunityName = driver.FindElement(By.CssSelector("[data-tid='Title']")).Text;
        displayedCommunityName.Should().Be(communityName);
        
    }
    
    // 5. Тест на изменение названия сообщества
    [Test]
    public void EditingCommunityNameTest()
    {
        // Авторизация
        Authorization();
        
        // Создание сообщества с уникальным названием
        addUniqueCommunity();
        
        // Переходим на главную страницу сообщества, нажимая кнопку "Закрыть"
        var closeCommunitySettingsButton = driver.FindElement(By.CssSelector("[class='sc-juXuNZ kVHSha']")); // больше не за что было зацепиться
        closeCommunitySettingsButton.Click();
        
        var currentUrl = driver.Url;
        driver.Navigate().GoToUrl(driver.Url+"/settings");
        
        // Генерируем уникальное название для сообщества, выводим его
        var communityName = Guid.NewGuid().ToString();
        Console.WriteLine("Новое название сообщества: " + communityName);
        
        // Находим поле Название сообщества, удаляем старое название
        var communityNameBar = driver.FindElement(By.CssSelector("[placeholder='Название сообщества']"));
        
        communityNameBar.SendKeys(Keys.Control + "a");
        communityNameBar.SendKeys(Keys.Delete);
        
        // Вводим новое сгенерированное название, нажимаем кнопку Сохранить
        communityNameBar.SendKeys(communityName);
        var saveCommunitySettingsButton = driver.FindElement(By.XPath("//*[text()='Сохранить']")); // иначе никак, классы одинаковые, tid-ов нет
        saveCommunitySettingsButton.Click();
        Thread.Sleep(4000);
        var wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
        wait2.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='Members']"))); // Тест падал в этом месте без явных ожиданий
        
        // Сравниваем название в заголовке созданного сообщества со сгенерированным значением
        var displayedCommunityName = driver.FindElement(By.CssSelector("[data-tid='Title']")).Text;
        displayedCommunityName.Should().Be(communityName);

    }
    
    // Выносим Авторизацию в метод
    public void Authorization()
    {
        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--start-maximized", "--disable-extensions");
       
        // Входим в Хром
        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
       
        // Переходим по урл https://staff-testing.testkontur.ru
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Username")));
        
        // Вводим логин и пароль
        var login = driver.FindElement(By.Id("Username"));
        login.SendKeys("a.o.klyukin@yandex.ru");
        var password = driver.FindElement(By.Name("Password"));
        password.SendKeys("IaMrEaDytony35!");
        
        // Нажимаем на кнопку "Войти"
        var enter = driver.FindElement(By.Name("button"));
        enter.Click();
        var news = driver.FindElement(By.CssSelector("[data-tid='Feed']"));

    }
    
    // Выносим создание сообщества с уникальным названием в метод
    public void addUniqueCommunity()
    {
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/communities");
        
        //Нажимаем кнопку Создать
        var addCommunityButton = driver.FindElement(By.CssSelector("[class='sc-juXuNZ sc-ecQkzk WTxfS vPeNx']")); // больше не за что было зацепиться
        addCommunityButton.Click();
        
        // Генерируем уникальное название для сообщества, выводим его
        var communityName = Guid.NewGuid().ToString();
        Console.WriteLine("Уникальное название сообщества: " + communityName);
        

        // Находим поле Название сообщества, вводим туда сгенерированное выше название
        var communityNameBar = driver.FindElement(By.CssSelector("[placeholder='Название сообщества']"));
        communityNameBar.SendKeys(communityName);
        
        // Нажимаем на кнопку Создать
        var createButton = driver.FindElement(By.CssSelector("[class='react-ui-m0adju']")); // больше не за что было зацепиться
        createButton.Click();
    }
    
    [TearDown]
    public void TearDown()
    {
        driver.Quit(); 
    }
}