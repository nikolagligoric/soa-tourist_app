import os
import pytest
import time
import uuid


from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.webdriver import WebDriver
from selenium.webdriver.support.ui import Select
import chromedriver_autoinstaller


"""
    instanca browsera
    koja ce se koristiti u svakom testu
"""


@pytest.fixture()
def browser():
    # instaliraj chrome driver
    # ukoliko ne postoji
    chromedriver_autoinstaller.install()

    # kreiraj instancu drajvera za test
    driver = webdriver.Chrome()

    driver.implicitly_wait(10)
    # Yield the WebDriver instance
    yield driver
    # Close the WebDriver instance
    driver.quit()


APPLICATION_URL = os.getenv("APPLICATION_URL", "http://localhost:4200")


class TestE2e:

    def test_registracija_turiste(self, browser: 'WebDriver'):
        
        browser.get(f"{APPLICATION_URL}/register")
        pause_step()

        first_name_input = browser.find_element(By.ID, "firstName")
        last_name_input = browser.find_element(By.ID, "lastName")
        username_input = browser.find_element(By.ID, "userName")
        email_input = browser.find_element(By.ID, "email")
        password_input = browser.find_element(By.ID, "password")
        role_select = Select(browser.find_element(By.ID, "role"))
        register_dugme = browser.find_element(
            By.XPATH, "//button[text()='Registruj se']")

        first_name_input.send_keys("Petar")
        pause_step()
        last_name_input.send_keys("Petrovic")
        pause_step()
        username_input.send_keys("petar")
        pause_step()
        email_input.send_keys("petarpetrovic@example.com")
        pause_step()
        password_input.send_keys("123")
        pause_step()
        role_select.select_by_value("Tourist")
        pause_step()
        register_dugme.click()
        pause_step()

        uspesna_registracija_poruka = browser.find_element(
            By.XPATH,
            "//div[contains(@class, 'alert-success')]"
        )

        assert uspesna_registracija_poruka.is_displayed()

    def test_dupla_registracija_prikazuje_gresku(self, browser: 'WebDriver'):

        self.registruj_korisnika(browser, "marko")
        time.sleep(2)

        browser.get(f"{APPLICATION_URL}/register")
        pause_step()

        browser.find_element(By.ID, "firstName").send_keys("Drugi")
        pause_step()
        browser.find_element(By.ID, "lastName").send_keys("Korisnik")
        pause_step()
        browser.find_element(By.ID, "userName").send_keys("marko")
        pause_step()
        browser.find_element(By.ID, "email").send_keys("DrugiKorisnik@example.com")
        pause_step()
        browser.find_element(By.ID, "password").send_keys("123")
        pause_step()
        Select(browser.find_element(By.ID, "role")).select_by_value("Tourist")
        pause_step()
        browser.find_element(By.XPATH, "//button[text()='Registruj se']").click()
        pause_step()

        greska_poruka = browser.find_element(
            By.XPATH,
            "//div[contains(@class, 'alert-danger')]"
        )

        assert greska_poruka.is_displayed()

    def test_uspesno_logovanje(self, browser: 'WebDriver'):
        self.registruj_korisnika(browser, "zika")
        time.sleep(2)

        browser.get(f"{APPLICATION_URL}/login")
        pause_step()

        username_input = browser.find_element(By.ID, "username")
        password_input = browser.find_element(By.ID, "password")
        login_dugme = browser.find_element(
            By.XPATH, "//button[text()='Prijavi se']")

        username_input.send_keys("zika")
        pause_step()
        password_input.send_keys("123")
        pause_step()
        login_dugme.click()
        pause_step()

        username_navbar = browser.find_element(
            By.XPATH,
            "//span[contains(@class, 'nav-username-text')]"
        )

        assert username_navbar.is_displayed()
        assert "zika" in username_navbar.text

    def test_neuspesno_logovanje(self, browser: 'WebDriver'):
        self.registruj_korisnika(browser, "mika")
        time.sleep(2)

        browser.get(f"{APPLICATION_URL}/login")
        pause_step()

        username_input = browser.find_element(By.ID, "username")
        password_input = browser.find_element(By.ID, "password")
        login_dugme = browser.find_element(
            By.XPATH, "//button[text()='Prijavi se']")

        username_input.send_keys("mika")
        pause_step()
        password_input.send_keys("pogresna")
        pause_step()
        login_dugme.click()
        pause_step()

        neispravno_logovanje_poruka = browser.find_element(
            By.XPATH,
            "//div[contains(@class, 'alert-danger')]"
        )

        assert neispravno_logovanje_poruka.is_displayed()

    def test_izmena_profila(self, browser: 'WebDriver'):
        self.registruj_korisnika(browser, "milica", "Milica", "Stara")
        time.sleep(2)
        self.uloguj_korisnika(browser, "milica")

        dropdown_trigger = browser.find_element(
            By.XPATH,
            "//div[contains(@class, 'dropdown-trigger')]"
        )
        dropdown_trigger.click()
        pause_step()

        moj_profil_dugme = browser.find_element(
            By.XPATH,
            "//button[contains(., 'Moj Profil')]"
        )
        moj_profil_dugme.click()
        pause_step()

        izmeni_profil_dugme = browser.find_element(
            By.XPATH,
            "//button[contains(., 'Izmeni profil')]"
        )
        izmeni_profil_dugme.click()
        pause_step()

        last_name_input = browser.find_element(By.NAME, "lastName")
        motto_input = browser.find_element(By.NAME, "motto")
        bio_input = browser.find_element(By.NAME, "bio")
        sacuvaj_dugme = browser.find_element(
            By.XPATH, "//button[contains(., 'uvaj izmene')]")
        

        last_name_input.clear()
        last_name_input.send_keys("Nova")
        pause_step()
        motto_input.clear()
        motto_input.send_keys("Novi Moto")
        pause_step()
        bio_input.clear()
        bio_input.send_keys("Nova biografija")
        pause_step()
        sacuvaj_dugme.click()
        pause_step()

        uspesna_izmena_poruka = browser.find_element(
            By.XPATH,
            "//div[contains(@class, 'alert-success')]"
        )
        biografija_box = browser.find_element(
            By.XPATH,
            "//div[contains(@class, 'profile-bio-box')]"
        )

        assert uspesna_izmena_poruka.is_displayed()
        assert "Nova biografija" in biografija_box.text

    def test_odjava_korisnika(self, browser: 'WebDriver'):
        self.registruj_korisnika(browser, "mladen")
        time.sleep(2)
        self.uloguj_korisnika(browser, "mladen")

        dropdown_trigger = browser.find_element(
            By.XPATH,
            "//div[contains(@class, 'dropdown-trigger')]"
        )
        dropdown_trigger.click()
        pause_step()

        logout_dugme = browser.find_element(
            By.XPATH,
            "//button[contains(., 'Odjavi se')]"
        )
        logout_dugme.click()
        pause_step()

        username_input = browser.find_element(By.ID, "username")

        assert username_input.is_displayed()
        assert "/login" in browser.current_url

    def registruj_korisnika(
            self,
            browser: 'WebDriver',
            username: str,
            first_name: str = "Petar",
            last_name: str = "Petrovic"):
        browser.get(f"{APPLICATION_URL}/register")
        pause_step()

        browser.find_element(By.ID, "firstName").send_keys(first_name)
        pause_step()
        browser.find_element(By.ID, "lastName").send_keys(last_name)
        pause_step()
        browser.find_element(By.ID, "userName").send_keys(username)
        pause_step()
        browser.find_element(By.ID, "email").send_keys(f"{username}@example.com")
        pause_step()
        browser.find_element(By.ID, "password").send_keys("123")
        pause_step()
        Select(browser.find_element(By.ID, "role")).select_by_value("Tourist")
        pause_step()
        browser.find_element(By.XPATH, "//button[text()='Registruj se']").click()
        pause_step()

    def uloguj_korisnika(self, browser: 'WebDriver', username: str):
        browser.get(f"{APPLICATION_URL}/login")
        pause_step()

        browser.find_element(By.ID, "username").send_keys(username)
        pause_step()
        browser.find_element(By.ID, "password").send_keys("123")
        pause_step()
        browser.find_element(By.XPATH, "//button[text()='Prijavi se']").click()
        pause_step()


def pause_step():
    step_delay = float(os.getenv("E2E_STEP_DELAY", "0"))
    if step_delay > 0:
        time.sleep(step_delay)
