document.addEventListener("DOMContentLoaded", function () {
    new Swiper(".pri-slider", {
        loop: true,
        effect: "fade", 
        speed: 1500,    
        autoplay: {
            delay: 4000,
            disableOnInteraction: false,
        },
        navigation: {
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
        },
        pagination: {
            el: ".swiper-pagination",
            clickable: true,
        },
    });

    new Swiper(".brand-logo-slider", {
        loop: true,
        speed: 6000, 
        allowTouchMove: false,
        autoplay: {
            delay: 0, 
            disableOnInteraction: false,
        },
        slidesPerView: 2,
        spaceBetween: 50,
        freeMode: true,
        breakpoints: {
            768: { slidesPerView: 4 },
            1024: { slidesPerView: 6 }
        }
    });
});