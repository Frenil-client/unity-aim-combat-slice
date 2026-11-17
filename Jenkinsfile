// unity-aim-combat-slice 빌드 파이프라인. Addressables 빌드, Windows 클라이언트, Android APK 를 만든다.
// 시작점은 Addressables Delivery 패키지의 CI 템플릿(Tools ▸ Addressables Delivery ▸ CI 파일 내보내기)이고 이 프로젝트에 맞게 고쳤다.
// - Addressables 그룹은 모두 앱에 들어가는 로컬 그룹이라 원격 업로드와 콘텐츠 업데이트 빌드는 없다. 플랫폼마다 전체 빌드를 하고 바로 플레이어를 빌드한다.
// - 테스트 단계는 두지 않는다(게임 기능이 정리될 때까지 테스트 보류, PLAN.md). 넣을 때는 템플릿의 EditMode, PlayMode 단계를 가져온다.
// 에이전트: Unity 6000.3.9f1(Android 모듈 포함)과 라이선스가 있는 Windows PC(레이블 unity-windows). 에이전트 환경 변수 UNITY_EXE 로 Unity 경로를 바꿀 수 있다.
// main 폴링으로 시작된 실행은 무결성 검사만 한다. 빌드는 사람이 파라미터를 골라 실행하고, 실행 중에 다시 묻지 않는다.
pipeline {
    agent { label 'unity-windows' }

    options {
        timestamps()
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '20', artifactNumToKeepStr: '5'))
    }

    triggers {
        pollSCM('H/15 * * * *')
    }

    parameters {
        choice(name: 'TARGET', choices: ['both', 'Windows', 'Android'], description: 'both: Windows 클라이언트와 Android APK 를 차례로 만든다')
        choice(name: 'STEPS', choices: ['addressables+player', 'addressables'], description: 'addressables+player: Addressables 전체 빌드 뒤 개발 빌드를 만들어 보관한다. addressables: 콘텐츠만 빌드한다')
    }

    stages {
        stage('무결성 검사') {
            steps {
                powershell 'ci/integrity.ps1'
            }
        }

        stage('Windows') {
            when { expression { isManualRun() && params.TARGET in ['both', 'Windows'] } }
            steps {
                buildPlatform('StandaloneWindows64')
            }
        }

        stage('Android') {
            when { expression { isManualRun() && params.TARGET in ['both', 'Android'] } }
            steps {
                buildPlatform('Android')
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'ci-logs/*.log', allowEmptyArchive: true
        }
    }
}

// 사람이 눌러 시작한 실행인지. main 폴링으로 시작된 실행은 검사만 한다.
def isManualRun() {
    return currentBuild.getBuildCauses('hudson.triggers.SCMTrigger$SCMTriggerCause').isEmpty()
}

// 한 플랫폼: Addressables 전체 빌드(LocalServer 프로필, 로컬 그룹은 앱에 들어간다) → 개발 빌드 → 보관.
// 플레이어 빌드는 Addressables 결과가 같은 플랫폼, 같은 프로필로 만들어졌는지 확인하고 아니면 거부한다(패키지의 빌드 검사).
def buildPlatform(String target) {
    def name = target == 'Android' ? 'android' : 'windows'
    unity("addressables-${name}", target, "'-executeMethod','AddressablesDelivery.Editor.CiBuild.Addressables','-deliveryProfile','LocalServer','-deliveryMode','full'")
    if (params.STEPS == 'addressables+player') {
        unity("player-${name}", target, "'-executeMethod','AddressablesDelivery.Editor.CiBuild.Player'")
        archivePlayer(target)
    }
}

// Unity 를 배치 모드로 한 번 실행한다. 로그는 ci-logs/<name>.log.
def unity(String name, String target, String arguments) {
    powershell """
        ci/unity.ps1 -Name ${name} -BuildTarget ${target} -Arguments ${arguments}
        exit \$LASTEXITCODE
    """
}

def archivePlayer(String target) {
    if (target == 'Android') {
        archiveArtifacts artifacts: 'Builds/Android/*.apk', fingerprint: true
    } else {
        powershell "Compress-Archive -Path Builds/Windows/* -DestinationPath ci-logs/Player-Windows.zip -Force"
        archiveArtifacts artifacts: 'ci-logs/Player-Windows.zip', fingerprint: true
    }
}
