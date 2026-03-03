import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { RouterOutlet } from '@angular/router';
import { EmailResponse, EmailService } from './services/email';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterOutlet], // ייבוא המודולים הדרושים
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  emailForm: FormGroup;
  serverResponse: EmailResponse | null = null;
  isRateLimited = false;

  showHiddenButton = false;
  testLogs: any[] = [];

  constructor(private fb: FormBuilder, private emailService: EmailService) {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  sendEmail() {
    
    if (this.emailForm.invalid) return;

    this.isRateLimited = false;
    const emailValue = this.emailForm.value.email;

    this.emailService.sendEmail(emailValue).subscribe({
      next: (res) => {
        this.serverResponse = res;
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 429) {
          this.isRateLimited = true;
          this.serverResponse = err.error; // server response
        }
      }
    });
  }

  // פונקציה להוספת שורת לוג עם זמן מקומי של הדפדפן
  private addLog(label: string, status: string, data: any) {
    const now = new Date();
    const timeStr = `${now.getHours()}:${now.getMinutes()}:${now.getSeconds()}`;
    this.testLogs.push({ time: timeStr, label, status, data });
  }

  runStressTest() {
    this.testLogs = []; // איפוס הלוג
    const testEmail = "test@tel-aviv.gov.il";

    // בקשה ראשונה
    this.emailService.sendEmail(testEmail).subscribe({
      next: (res) => this.addLog("Request 1", "SUCCESS", res),
      error: (err) => this.addLog("Request 1", "ERROR " + err.status, err.error)
    });

    // בקשה שנייה - נשלחת מיד (ללא המתנה)
    this.emailService.sendEmail(testEmail).subscribe({
      next: (res) => this.addLog("Request 2", "SUCCESS", res),
      error: (err) => {
        const statusLabel = err.status === 429 ? "FAILED (Rate Limited)" : "ERROR " + err.status;
        this.addLog("Request 2", statusLabel, err.error);
      }
    });
  }
}
