#!/usr/bin/env python3
"""Wrapper regression tests in a temporary checkout, using a fake executable.
No Unity, Android toolchain, or real APK is used or validated by this test.
"""
from pathlib import Path
import os
import shutil
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]


class WrapperTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='rumores-wrapper-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        (self.root / 'Tools').mkdir()
        (self.root / 'Builds').mkdir()
        shutil.copyfile(ROOT / 'Tools/unity.sh', self.root / 'Tools/unity.sh')
        self.artifact = self.root / 'Builds/Rumores-dev.apk'
        self.fake = self.root / 'fake-editor'

    def run_editor(self, body, mode='android'):
        self.fake.write_text('#!/usr/bin/env bash\nset -eu\n' + body + '\n')
        self.fake.chmod(0o700)
        return subprocess.run(['bash', str(self.root / 'Tools/unity.sh'), mode],
                              env={**os.environ, 'UNITY_EDITOR': str(self.fake)},
                              capture_output=True, text=True)

    def test_stale_artifact_is_not_accepted(self):
        self.artifact.write_bytes(b'old placeholder')
        result = self.run_editor('exit 0')
        self.assertNotEqual(result.returncode, 0)
        self.assertFalse(self.artifact.exists())

    def test_failure_preserves_exit_code_and_removes_partial_output(self):
        result = self.run_editor("printf 'partial placeholder' > Builds/Rumores-dev.apk\nexit 7")
        self.assertEqual(result.returncode, 7)
        self.assertFalse(self.artifact.exists())

    def test_new_output_requires_successful_process(self):
        result = self.run_editor("printf 'test placeholder, NOT an APK' > Builds/Rumores-dev.apk", 'apk')
        self.assertEqual(result.returncode, 0, result.stderr)

    def test_empty_output_fails(self):
        self.assertNotEqual(self.run_editor(': > Builds/Rumores-dev.apk').returncode, 0)

    def test_stale_test_xml_is_not_accepted(self):
        (self.root / 'TestResults').mkdir()
        path = self.root / 'TestResults/editmode.xml'
        path.write_text('<test-run total="1" failed="0" result="Passed"/>')
        self.assertNotEqual(self.run_editor('exit 0', 'editmode').returncode, 0)
        self.assertFalse(path.exists())

    def test_zero_tests_are_rejected(self):
        result = self.run_editor('echo \'<test-run total="0" failed="0" result="Passed"/>\' > TestResults/editmode.xml', 'editmode')
        self.assertNotEqual(result.returncode, 0)

    def test_current_passing_xml_is_accepted(self):
        result = self.run_editor('echo \'<test-run total="2" failed="0" result="Passed"/>\' > TestResults/playmode.xml', 'playmode')
        self.assertEqual(result.returncode, 0, result.stderr)

    def test_signed_bundle_has_no_cli_secret_route(self):
        self.assertEqual(self.run_editor('exit 0', 'aab').returncode, 2)


if __name__ == '__main__':
    unittest.main(verbosity=2)
