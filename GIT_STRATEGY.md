# Git Strategy: ico Project

This document outlines the Git workflow and standards for the `ico` repository to ensure a clean, traceable, and collaborative development environment.

## 1. Branching Model

We follow a strict **Feature Branch** model based on a protected `main` branch.

### Primary Branches
- **`main`**: **PROTECTED (Pull-Only for Humans).** No direct commits are allowed. This branch represents stable, production-ready code.
- **`dev`**: The primary integration branch. All feature work is merged here via Pull Requests before eventually being promoted to `main`.

### Supporting Branches
- **`feature/`**: **Mandatory for all new work.** Branches must be created from `dev` (e.g., `feature/multi-res-extraction`).
- **`fix/`**: Used for bug fixes, also created from `dev`.
- **`docs/`**: Used for documentation-only changes.

## 2. Worktree Strategy

To maintain focus and avoid frequent context switching (stashing/unstashing), we utilize Git Worktrees:

- **Root (`/ico`)**: Dedicated to the `main` branch. **Used for syncing/pulling only.**
- **Dev Tree (`/ico-devtree`)**: Dedicated to the `dev` branch and for creating feature branches.

## 3. Commit Message Convention

We use a lightweight version of **Conventional Commits**:
`<type>: <description>`

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation changes
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **test**: Adding or correcting tests

## 4. Mandatory Workflow (Plan -> Act -> Validate)

1.  **Branch Creation**: Always create a new `feature/` branch from the latest `dev`.
    ```powershell
    git checkout dev
    git pull origin dev
    git checkout -b feature/your-feature-name
    ```
2.  **Implementation**: Work only within your feature branch.
3.  **Commit**: Commit your changes using the naming convention.
4.  **Integration (PR)**: Create a Pull Request (PR) to merge your feature branch back into `dev`. **Do not merge locally.**
5.  **Hygiene & Cleanup**: Once the PR is merged into `dev`:
    - Pull the updated `dev` locally.
    - Delete the local and remote feature branch to maintain repository hygiene.
    ```powershell
    git branch -d feature/your-feature-name
    git push origin --delete feature/your-feature-name
    ```

## 5. Remote Sync & Protection

- **Main Protection**: Never `git push origin main`. Only `git pull` is permitted to sync your local environment with the production state.
- **PR Requirement**: All changes to `dev` and `main` must pass through the GitHub PR interface to ensure auditability and quality control.
