import os
import time

import chromedriver_autoinstaller
import pytest
from selenium import webdriver
from selenium.webdriver import ActionChains
from selenium.webdriver.chrome.webdriver import WebDriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import Select, WebDriverWait


APPLICATION_URL = os.getenv("APPLICATION_URL", "http://localhost:4200")
GUIDE_USERNAME = os.getenv("GUIDE_USERNAME", "vodic")
GUIDE_PASSWORD = os.getenv("GUIDE_PASSWORD", "123")
TOURIST_USERNAME = os.getenv("TOURIST_USERNAME", "turista")
TOURIST_PASSWORD = os.getenv("TOURIST_PASSWORD", "123")
STEP_DELAY_SECONDS = float(os.getenv("E2E_STEP_DELAY_SECONDS", "3"))
E2E_RUN_ID = int(time.time())
E2E_TOUR_NAME = f"E2E Tour {E2E_RUN_ID}"
PURCHASED_TOUR_ID = None


@pytest.fixture()
def browser():
    chromedriver_autoinstaller.install()
    driver = webdriver.Chrome()
    driver.maximize_window()
    driver.implicitly_wait(5)
    yield driver
    time.sleep(STEP_DELAY_SECONDS)
    driver.quit()


def wait(browser: WebDriver):
    return WebDriverWait(browser, 15)


def step_pause():
    if STEP_DELAY_SECONDS > 0:
        time.sleep(STEP_DELAY_SECONDS)


def click_after_pause(element):
    step_pause()
    element.click()


def by_test_id(test_id: str):
    return (By.CSS_SELECTOR, f"[data-testid='{test_id}']")


def login(browser: WebDriver, username: str, password: str):
    browser.get(f"{APPLICATION_URL}/login")
    wait(browser).until(EC.visibility_of_element_located(by_test_id("login-username"))).send_keys(username)
    browser.find_element(*by_test_id("login-password")).send_keys(password)
    click_after_pause(browser.find_element(*by_test_id("login-submit")))
    wait(browser).until(EC.url_contains("/home"))
    step_pause()


def create_draft_tour(browser: WebDriver, name: str):
    browser.get(f"{APPLICATION_URL}/author/tours")
    wait(browser).until(EC.visibility_of_element_located(by_test_id("tour-name"))).send_keys(name)
    browser.find_element(*by_test_id("tour-description")).send_keys("E2E opis ture")
    Select(browser.find_element(*by_test_id("tour-difficulty"))).select_by_visible_text("EASY")
    browser.find_element(*by_test_id("tour-tags")).send_keys("e2e, test")
    click_after_pause(browser.find_element(*by_test_id("create-tour-button")))
    wait(browser).until(EC.text_to_be_present_in_element((By.TAG_NAME, "body"), "Tura je kreirana kao draft"))
    step_pause()


def select_tour(browser: WebDriver, name: str):
    tour_button = wait(browser).until(EC.element_to_be_clickable((By.XPATH, f"//button[contains(@class,'tour-row')]//strong[text()='{name}']/ancestor::button")))
    click_after_pause(tour_button)
    wait(browser).until(EC.visibility_of_element_located((By.ID, "authorTourMap")))
    step_pause()


def click_author_map(browser: WebDriver, x_offset: int, y_offset: int):
    map_element = wait(browser).until(EC.visibility_of_element_located((By.ID, "authorTourMap")))
    browser.execute_script("arguments[0].scrollIntoView({block: 'center'});", map_element)
    step_pause()

    width = map_element.size["width"]
    height = map_element.size["height"]
    safe_x = min(max(x_offset, 10), max(width - 10, 10))
    safe_y = min(max(y_offset, 10), max(height - 10, 10))

    ActionChains(browser).move_to_element_with_offset(map_element, safe_x, safe_y).click().perform()
    step_pause()


def add_keypoint(browser: WebDriver, name: str, x_offset: int, y_offset: int):
    click_author_map(browser, x_offset, y_offset)
    browser.find_element(*by_test_id("keypoint-name")).clear()
    browser.find_element(*by_test_id("keypoint-name")).send_keys(name)
    browser.find_element(*by_test_id("keypoint-description")).clear()
    browser.find_element(*by_test_id("keypoint-description")).send_keys(f"Opis za {name}")
    browser.find_element(*by_test_id("keypoint-image-url")).clear()
    browser.find_element(*by_test_id("keypoint-image-url")).send_keys("https://example.com/image.jpg")
    click_after_pause(browser.find_element(*by_test_id("add-keypoint-button")))
    wait(browser).until(EC.text_to_be_present_in_element((By.TAG_NAME, "body"), "Kljucna tacka je dodata turi"))
    step_pause()


def button_for_tour(browser: WebDriver, tour_name: str, test_id: str):
    xpath = (
        f"//article[@data-tour-name='{tour_name}']"
        f"//*[@data-testid='{test_id}']"
    )
    return wait(browser).until(EC.element_to_be_clickable((By.XPATH, xpath)))


def start_button_for_purchased_tour(browser: WebDriver, tour_id: str):
    xpath = (
        f"//article[contains(@class,'purchase-card')]"
        f"[.//span[contains(normalize-space(), 'Tour ID: {tour_id}')]]"
        f"//*[@data-testid='start-tour-button']"
    )
    return wait(browser).until(EC.element_to_be_clickable((By.XPATH, xpath)))


def cart_row_for_tour(browser: WebDriver, tour_id: str):
    return wait(browser).until(
        EC.visibility_of_element_located((By.CSS_SELECTOR, f".table-row[data-tour-id='{tour_id}']"))
    )


class TestSoaTouristAppE2e:
    def test_01_guide_creates_draft_tour(self, browser: WebDriver):
        login(browser, GUIDE_USERNAME, GUIDE_PASSWORD)
        create_draft_tour(browser, E2E_TOUR_NAME)
        step_pause()

        assert E2E_TOUR_NAME in browser.page_source
        assert "DRAFT" in browser.page_source

    def test_02_guide_adds_keypoints_duration_and_publishes_tour(self, browser: WebDriver):
        login(browser, GUIDE_USERNAME, GUIDE_PASSWORD)
        browser.get(f"{APPLICATION_URL}/author/tours")
        select_tour(browser, E2E_TOUR_NAME)
        add_keypoint(browser, "E2E tacka 1", 40, 40)
        add_keypoint(browser, "E2E tacka 2", 120, 80)

        browser.find_element(*by_test_id("duration-minutes")).clear()
        browser.find_element(*by_test_id("duration-minutes")).send_keys("90")
        click_after_pause(browser.find_element(*by_test_id("add-duration-button")))
        wait(browser).until(EC.text_to_be_present_in_element((By.TAG_NAME, "body"), "Vreme obilaska je dodato"))
        step_pause()

        click_after_pause(browser.find_element(*by_test_id("publish-tour-button")))
        wait(browser).until(EC.text_to_be_present_in_element((By.TAG_NAME, "body"), "Tura je poslata na objavljivanje"))
        step_pause()

        assert E2E_TOUR_NAME in browser.page_source

    def test_03_tourist_buys_tour(self, browser: WebDriver):
        global PURCHASED_TOUR_ID

        login(browser, TOURIST_USERNAME, TOURIST_PASSWORD)
        browser.get(f"{APPLICATION_URL}/tours")

        add_button = button_for_tour(browser, E2E_TOUR_NAME, "add-to-cart-button")
        tour_card = add_button.find_element(By.XPATH, "./ancestor::article[contains(@class,'tour-card')]")
        PURCHASED_TOUR_ID = tour_card.get_attribute("data-tour-id")
        assert PURCHASED_TOUR_ID, "Kartica ture nema data-tour-id."
        click_after_pause(add_button)
        wait(browser).until(
            lambda driver: "tura je" in driver.find_element(By.TAG_NAME, "body").text.lower()
            or "already" in driver.find_element(By.TAG_NAME, "body").text.lower()
            or "cart" in driver.find_element(By.TAG_NAME, "body").text.lower()
            or "korpu" in driver.find_element(By.TAG_NAME, "body").text.lower()
            or "korpi" in driver.find_element(By.TAG_NAME, "body").text.lower()
        )
        step_pause()

        browser.get(f"{APPLICATION_URL}/cart")
        cart_row_for_tour(browser, PURCHASED_TOUR_ID)
        step_pause()

        click_after_pause(wait(browser).until(EC.visibility_of_element_located(by_test_id("checkout-button"))))
        wait(browser).until(EC.text_to_be_present_in_element((By.TAG_NAME, "body"), "uspe"))
        step_pause()

        assert "Moja korpa" in browser.page_source

    def test_04_tourist_starts_tour_and_checks_location(self, browser: WebDriver):
        assert PURCHASED_TOUR_ID is not None, "Pre pokretanja ture mora proci test kupovine."

        login(browser, TOURIST_USERNAME, TOURIST_PASSWORD)

        browser.get(f"{APPLICATION_URL}/current-location")
        location_map = wait(browser).until(EC.visibility_of_element_located(by_test_id("current-location-map")))
        ActionChains(browser).move_to_element_with_offset(location_map, 80, 80).click().perform()
        click_after_pause(wait(browser).until(EC.element_to_be_clickable(by_test_id("save-current-location-button"))))
        wait(browser).until(EC.text_to_be_present_in_element((By.TAG_NAME, "body"), "Trenutna lokacija je sacuvana"))
        step_pause()

        browser.get(f"{APPLICATION_URL}/purchases")

        start_button = start_button_for_purchased_tour(browser, PURCHASED_TOUR_ID)
        click_after_pause(start_button)
        wait(browser).until(EC.url_contains("/active-tour"))
        wait(browser).until(EC.text_to_be_present_in_element((By.TAG_NAME, "body"), "Aktivna tura"))
        step_pause()

        if not browser.find_elements(*by_test_id("execution-latitude")):
            page_text = browser.find_element(By.TAG_NAME, "body").text
            assert False, (
                "TourExecution nije startovan ili se execution forma nije prikazala. "
                f"Tekst stranice:\n{page_text}"
            )

        latitude = wait(browser).until(EC.visibility_of_element_located(by_test_id("execution-latitude")))
        latitude.clear()
        latitude.send_keys("45.2551")
        longitude = browser.find_element(*by_test_id("execution-longitude"))
        longitude.clear()
        longitude.send_keys("19.8452")

        click_after_pause(browser.find_element(*by_test_id("save-execution-location-button")))
        wait(browser).until(EC.text_to_be_present_in_element((By.TAG_NAME, "body"), "Lokacija"))
        step_pause()

        click_after_pause(browser.find_element(*by_test_id("check-location-button")))
        wait(browser).until(
            lambda driver: "Provera" in driver.page_source
            or "kljuc" in driver.page_source
            or "lokacije" in driver.page_source
        )
        step_pause()

        assert "ACTIVE" in browser.page_source or "Aktivna tura" in browser.page_source

        click_after_pause(browser.find_element(*by_test_id("complete-tour-button")))
        wait(browser).until(
            lambda driver: "COMPLETED" in driver.page_source
            or "uspe" in driver.page_source.lower()
            or "zavrs" in driver.page_source.lower()
        )
        step_pause()
