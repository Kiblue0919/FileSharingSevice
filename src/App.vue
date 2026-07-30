<template>
  <div class="app">
    <header class="navbar">
      <div class="navbar-container">
        <router-link to="/" class="logo logo-link">
          <span class="logo-icon">📁</span>
          <span class="logo-text">FileShare</span>
        </router-link>
        <nav class="nav-links">
          <router-link to="/" class="nav-link">Upload</router-link>
          <router-link to="/history" class="nav-link">History</router-link>
        </nav>
      </div>
    </header>
    <router-view />
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
}

.navbar {
  padding: 14px 20px;
}

.navbar-container {
  max-width: 1100px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.logo {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 18px;
  font-weight: 700;
  color: white;
}

.logo-link {
  text-decoration: none;
}

.logo-link:hover {
  opacity: 0.9;
}

.nav-links {
  display: flex;
  align-items: center;
  gap: 20px;
}

.nav-link {
  color: white;
  text-decoration: none;
  font-weight: 600;
  font-size: 14px;
  opacity: 0.9;
}

.nav-link:hover,
.nav-link.router-link-active {
  opacity: 1;
  text-decoration: underline;
  text-underline-offset: 4px;
}

@media (max-width: 640px) {
  .navbar-container {
    flex-wrap: wrap;
    justify-content: center;
  }

  .logo {
    width: 100%;
    justify-content: center;
  }
}
</style>