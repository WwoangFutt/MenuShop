// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('mainContent');
    const footer = document.querySelector('footer');

    function openSidebarMobile() {
        sidebar.classList.add('expanded');
    }
    function closeSidebarMobile() {
        sidebar.classList.remove('expanded');
    }
    function toggleSidebarDesktop() {
        sidebar.classList.toggle('collapsed');
        if (mainContent) mainContent.classList.toggle('collapsed');
        if (footer) footer.classList.toggle('collapsed');
    }
    function handleResize() {
        if (window.innerWidth > 768) {
            closeSidebarMobile();
        }
    }
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function(e) {
            e.stopPropagation();
            if (window.innerWidth >= 769) {
                toggleSidebarDesktop();
            } else {
                if (sidebar.classList.contains('expanded')) {
                    closeSidebarMobile();
                } else {
                    openSidebarMobile();
                }
            }
        });
    }
    // Đóng sidebar khi click ra ngoài trên mobile
    document.addEventListener('click', function(e) {
        if (window.innerWidth <= 768) {
            if (sidebar.classList.contains('expanded') && !sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                closeSidebarMobile();
            }
        }
    });
    window.addEventListener('resize', handleResize);

    // Scrollspy cho danh mục ngang
    const navLinks = document.querySelectorAll('.category-nav-link');
    const sections = Array.from(document.querySelectorAll('.category-section'));
    let isClickingNav = false;
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            navLinks.forEach(l => l.classList.remove('active'));
            this.classList.add('active');
            isClickingNav = true;
            setTimeout(() => { isClickingNav = false; }, 600);
            const id = this.getAttribute('href').replace('#', '');
            const section = document.getElementById(id);
            if (section) {
                let headerHeight = document.querySelector('.main-topbar')?.offsetHeight || 48;
                let catBarHeight = document.querySelector('.category-bar-sticky')?.offsetHeight || 56;
                let offset = headerHeight + catBarHeight + 10;
                window.scrollTo({
                    top: section.offsetTop - offset,
                    behavior: 'smooth'
                });
            }
        });
    });
    function onScroll() {
        if (isClickingNav) return;
        let scrollPos = window.scrollY || window.pageYOffset;
        let headerHeight = document.querySelector('.main-topbar')?.offsetHeight || 48;
        let catBarHeight = document.querySelector('.category-bar-sticky')?.offsetHeight || 56;
        let offset = headerHeight + catBarHeight + 10;
        let found = false;
        // Nếu ở trên section đầu tiên
        if (sections.length && scrollPos + offset < sections[0].offsetTop) {
            navLinks.forEach(link => link.classList.remove('active'));
            navLinks[0].classList.add('active');
            found = true;
        } else {
            for (let i = sections.length - 1; i >= 0; i--) {
                const section = sections[i];
                if (scrollPos + offset >= section.offsetTop) {
                    navLinks.forEach(link => link.classList.remove('active'));
                    const activeLink = document.querySelector('.category-nav-link[data-category-id="' + section.id.replace('category-', '') + '"]');
                    if (activeLink) activeLink.classList.add('active');
                    found = true;
                    break;
                }
            }
        }
        // Nếu cuộn gần cuối trang, luôn set active cho danh mục cuối cùng
        if (window.innerHeight + window.scrollY >= document.body.offsetHeight - 10) {
            navLinks.forEach(link => link.classList.remove('active'));
            navLinks[navLinks.length - 1].classList.add('active');
            found = true;
        }
        if (!found) navLinks.forEach(link => link.classList.remove('active'));
    }
    window.addEventListener('scroll', onScroll);
    onScroll();
});
