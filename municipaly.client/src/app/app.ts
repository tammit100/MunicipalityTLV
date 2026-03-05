import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { RouterOutlet } from '@angular/router';
import { EmailResponse, EmailService } from './services/email';
import { timer } from 'rxjs';
import { concatMap } from 'rxjs/operators'
import { signal } from '@angular/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterOutlet], // ייבוא המודולים הדרושים
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  emailForm: FormGroup;
  isRateLimited = signal<boolean>(false);
  serverResponse = signal<EmailResponse | null>(null);
  isTestMode = false;

  // מחליף את מצב הטסט (מופעל בלחיצה על T)

  showHiddenButton = false;
  testLogs: any[] = [];

  constructor(private fb: FormBuilder, private emailService: EmailService) {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  sendEmail() {
    
    if (this.emailForm.invalid) return;

    this.isRateLimited.set(false);
    const emailValue = this.emailForm.value.email;

    this.emailService.sendEmail(emailValue).subscribe({
      next: (res) => {
        this.serverResponse.set(res);
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 429) {
          this.isRateLimited.set(true);
          this.serverResponse.set(err.error); // המידע שה-Interceptor העביר הלאה
        }
      }
    });
  }

  // פונקציה להוספת שורת לוג עם זמן מקומי של הדפדפן
  private addLog(label: string, status: string, data: any) {
    const now = new Date();
    const timeStr = `${now.getHours()}:${now.getMinutes()}:${now.getSeconds()}`;
    this.testLogs.unshift({ time: timeStr, label, status, data });
  }

  runStressTest() {
    this.testLogs = []; // איפוס הלוג
    const testEmailSuccess = "testSuccess@tel-aviv.gov.il";
    const testEmailFail = "testFail@tel-aviv.gov.il";

    this.addLog("System", "INFO", `Starting Request 1...${testEmailSuccess}`);
    // בקשה ראשונה
    this.emailService.sendEmail(testEmailSuccess).subscribe({
      next: (res) => {
        this.addLog("Request 1", "SUCCESS (200 OK)", res);
        
        // כאן אנחנו מייצרים את ההמתנה של השניה
        this.addLog("System", "WAITING", "Waiting 1 second before Request 2...");
        
        timer(1000).subscribe(() => {
          // בקשה שנייה - תישלח בדיוק אחרי שניה
          this.addLog("System", "INFO", `Starting Request 1...${testEmailFail}`);
          
          this.emailService.sendEmail(testEmailFail).subscribe({
            next: (res2) => this.addLog("Request 2", "SUCCESS (Unexpected!)", res2),
            error: (err: HttpErrorResponse) => {
              const is429 = err.status === 429;
              const statusText = is429 ? "FAILED (429 Too Many Requests)" : `ERROR ${err.status}`;
    
              this.addLog("Request 2", statusText, err.error);

              // אם אתה משתמש ב-Signals, זה הזמן לעדכן גם אותם כדי שהמסך הראשי יתעדכן
              this.isRateLimited.set(is429);
              this.serverResponse.set(err.error);
            }
          });
        });
      },
      error: (err) => this.addLog("Request 1", "FAILED", err.error)
    });
  }

  clearEmailField() {
    // initialize the email field to empty string
    this.emailForm.get('email')?.setValue('');
    
    // mark the email field as untouched to reset validation state
    this.emailForm.get('email')?.markAsUntouched();
    
    // initialize server response and rate limit state
    this.serverResponse.set(null);
    this.isRateLimited.set(false);
  }

  deleteTestLogs(){
    this.testLogs = ["הלוג נמחק בהצלחה!"];
  }

  toggleStressMode() {
    this.showHiddenButton = !this.showHiddenButton
  } 

  toggleTestMode() {
    this.isTestMode = !this.isTestMode;
  }

}
