@echo off
REM ===========================================
REM Trợ Lý KOC - AI Worker Setup (Windows)
REM ===========================================

echo ============================================================
echo   Tro Ly KOC - AI Worker Setup
echo ============================================================
echo.

REM Check if venv exists
if not exist "venv" (
    echo [1/5] Creating virtual environment...
    python -m venv venv
) else (
    echo [1/5] Virtual environment already exists
)

REM Activate venv
echo [2/5] Activating virtual environment...
call venv\Scripts\activate.bat

REM Upgrade pip
echo [3/5] Upgrading pip...
python -m pip install --upgrade pip wheel setuptools

REM Install PyTorch with CUDA first
echo [4/5] Installing PyTorch with CUDA 12.1...
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121

REM Install other requirements
echo [5/5] Installing other dependencies...
pip install -r requirements.txt

echo.
echo ============================================================
echo   Installation Complete!
echo ============================================================
echo.
echo OPTIONAL: To enable FaceSwap, install insightface:
echo.
echo   Option A: If you have Visual Studio Build Tools:
echo     pip install insightface
echo.
echo   Option B: Use pre-built wheel (recommended):
echo     Download from: https://github.com/Gourieff/Assets/releases
echo     pip install insightface-0.7.3-cp311-cp311-win_amd64.whl
echo.
echo To run the worker:
echo     python main.py
echo.
pause
