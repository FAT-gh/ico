# Git Strategy: ico Project

This document outlines the Git workflow and standards for the `ico` repository to ensure a clean, traceable, and collaborative development environment.

## 1. Branching Model

We follow a simplified **Git Flow** model optimized for the use of **Git Worktrees**.

### Primary Branches
- **`main`**: The stable, production-ready branch. Only fully tested and reviewed code should be merged here.
- **`dev`**: The integration branch for ongoing development. This is where features are combined before moving to `main`.

### Supporting Branches
- **`feature/`**: Used for new features (e.g., `feature/add-bmp-support`).
- **`fix/`**: Used for bug fixes (e.g., `fix/path-encoding-issue`).
- **`docs/`**: Used for documentation-only changes.

## 2. Worktree Strategy

To maintain focus and avoid frequent context switching (stashing/unstashing), we utilize Git Worktrees:

- **Root (`/ico`)**: Dedicated to the `main` branch. Use this for final verification and releases.
- **Dev Tree (`/ico-devtree`)**: Dedicated to the `dev` branch. All active development and feature integration happens here.

### Creating a Feature Worktree
If a feature requires a long-lived isolated environment:
```powershell
git worktree add ../ico-feature-name feature/branch-name
```

## 3. Commit Message Convention

We use a lightweight version of **Conventional Commits**:
`<type>: <description>`

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation changes
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **test**: Adding or correcting tests

*Example: `feat: add support for multi-resolution icon extraction`*

## 4. Workflow Steps

1.  **Start Work**: Navigate to the `ico-devtree` directory.
2.  **Sync**: Always `git pull origin dev` before starting.
3.  **Implement**: Make changes, run local tests (PowerShell memory loading).
4.  **Commit**: `git add .` and `git commit -m "type: description"`.
5.  **Push**: `git push origin dev`.
6.  **Release**: When `dev` is stable, merge into `main` (via PR or local merge if authorized).

## 5. Remote Sync (GitHub)

- **Remote Name**: `origin`
- **URL**: `https://github.com/FAT-gh/ico.git`
- **Syncing**: Use `gh repo sync` or `git pull --rebase` to keep local environments clean.
