var swiper = new Swiper(".myCategorySwiper", {
    loop: true,
    navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
    },
    slidesPerView: 4,
    slidesPerGroup: 3,  // سيتحرك السلايدر 3 عناصر في كل مرة
});