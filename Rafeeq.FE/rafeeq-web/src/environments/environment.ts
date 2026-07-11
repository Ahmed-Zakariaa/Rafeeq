// Rafeeq FE → BE connection (LOCAL / development).
// Same pattern as ECM: API_URL + API_BASE_URL.
//
// Local-only override tip (mirrors ECM): after copying this into the Angular app, mark it
// git skip-worktree so your local API URL is never committed/pushed:
//   git update-index --skip-worktree src/environments/environment.ts
// To undo and pull updates later:
//   git update-index --no-skip-worktree src/environments/environment.ts

export const environment = {
  production: false,

  // Local backend (Rafeeq.Api project, http profile → port 5200). Controllers live under /api.
  API_URL: 'http://localhost:5200/api',
  API_BASE_URL: 'http://localhost:5200/',

  // Remote/hosted (committed default — uncomment when deployed, comment the local pair above):
  // API_URL: 'https://<your-host>/api',
  // API_BASE_URL: 'https://<your-host>/',
};
