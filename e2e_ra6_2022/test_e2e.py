import os
import time
import pytest

from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.webdriver import WebDriver
import chromedriver_autoinstaller
from selenium.webdriver.support.ui import WebDriverWait

@pytest.fixture()
def browser():
    chromedriver_autoinstaller.install()

    driver = webdriver.Chrome()
    driver.implicitly_wait(10)

    yield driver

    driver.quit()


APPLICATION_URL = os.getenv("APPLICATION_URL", "http://localhost:4200")


class TestE2E:

    def uloguj_korisnika(self, browser: WebDriver):
        browser.get(f"{APPLICATION_URL}/login")
        pause_step()

        browser.find_element(By.ID, "username").send_keys("nikola")
        pause_step()

        browser.find_element(By.ID, "password").send_keys("123")
        pause_step()

        browser.find_element(
            By.XPATH,
            "//button[contains(., 'Prijavi se')]"
        ).click()
        pause_step()


    def otvori_profil(self, browser: WebDriver):

        dropdown = browser.find_element(
            By.CLASS_NAME,
            "dropdown-trigger"
        )
        dropdown.click()
        pause_step()

        browser.find_element(
            By.XPATH,
            "//button[contains(., 'Moj Profil')]"
        ).click()
        pause_step()


    def test_pregled_profila(self, browser: WebDriver):

        self.uloguj_korisnika(browser)
        self.otvori_profil(browser)

        username = browser.find_element(
            By.XPATH,
            "//p[contains(@class,'profile-username')]"
        )

        assert username.is_displayed()
        assert "nikola" in username.text


    def test_izmena_profila(self, browser: WebDriver):

        self.uloguj_korisnika(browser)
        self.otvori_profil(browser)

        browser.find_element(
            By.XPATH,
            "//button[contains(., 'Izmeni profil')]"
        ).click()
        pause_step()

        bio = browser.find_element(By.NAME, "bio")
        bio.clear()
        bio.send_keys("E2E test bio izmena")
        pause_step()

        browser.find_element(
            By.XPATH,
            "//button[contains(., 'Sačuvaj izmene')]"
        ).click()
        pause_step()

        bio_text = browser.find_element(
            By.XPATH,
            "//div[contains(@class,'profile-bio-box')]"
        )

        assert "E2E test bio izmena" in bio_text.text

    from selenium.webdriver.support.ui import WebDriverWait

    def test_komentarisanje_bloga_pracenog_autora(self, browser: WebDriver):

        self.uloguj_korisnika(browser)

        browser.get(f"{APPLICATION_URL}/blogs")

        wait = WebDriverWait(browser, 10)

        first_blog = wait.until(
            lambda d: d.find_element(
                By.XPATH,
                "(//article[contains(@class,'blog-card')])[1]"
            )
        )

        first_blog.find_element(
            By.XPATH,
            ".//button[contains(., 'Prikazi komentare')]"
        ).click()

        comment_text = "E2E komentar autora kojeg pratim!"

        textarea = wait.until(
            lambda d: first_blog.find_element(
                By.XPATH,
                ".//textarea"
            )
        )

        textarea.send_keys(comment_text)

        first_blog.find_element(
            By.XPATH,
            ".//button[contains(., 'Komentarisi')]"
        ).click()

        wait.until(
            lambda d: any(
                comment_text in el.text
                for el in first_blog.find_elements(
                    By.XPATH,
                    ".//div[contains(@class,'comment-item')]"
                )
            )
        )

        comments = first_blog.find_elements(
            By.XPATH,
            ".//div[contains(@class,'comment-item')]"
        )

        assert any(comment_text in c.text for c in comments)


    def test_zapracivanje_iz_preporuka(self, browser: WebDriver):

        self.uloguj_korisnika(browser)

        browser.get(f"{APPLICATION_URL}/home")
        pause_step()

        rec_items = browser.find_elements(By.CLASS_NAME, "rec-item")
        assert len(rec_items) > 0

        first_item = rec_items[0]

        username = first_item.find_element(By.CLASS_NAME, "rec-username").text

        first_item.find_element(By.CLASS_NAME, "rec-follow-btn").click()

        wait = WebDriverWait(browser, 10)

        wait.until(
            lambda d: username not in [
                el.text for el in d.find_elements(By.CLASS_NAME, "rec-username")
            ]
        )

        updated_usernames = [
            item.find_element(By.CLASS_NAME, "rec-username").text
            for item in browser.find_elements(By.CLASS_NAME, "rec-item")
        ]

        assert username not in updated_usernames


def pause_step():
    step_delay = float(os.getenv("E2E_STEP_DELAY", "0"))
    if step_delay > 0:
        time.sleep(step_delay)