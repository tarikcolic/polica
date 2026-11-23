import { Component, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.scss'
})
export class AppComponent {
  private translate = inject(TranslateService);

  protected readonly title = 'Polica';

  constructor() {
    // Inicijalizacija translate servisa
    this.translate.addLangs(['en', 'bs']);
    this.translate.setFallbackLang('bs');

    // Polica (#5): <html lang="..."> follows the chosen text language (index.html starts with "bs").
    // Screen readers then pronounce the text in the right language, and the browser does not offer to
    // translate a Bosnian page "from English". Subscribed before use() below, so the first language
    // is applied too. AppComponent lives as long as the app, so the subscription needs no cleanup.
    this.translate.onLangChange.subscribe(event => {
      document.documentElement.lang = event.lang;
    });

    // Učitaj jezik iz localStorage ili koristi default
    const savedLang = localStorage.getItem('language') || 'bs';

    this.translate.use(savedLang).subscribe({
      error: (error) => {
        console.error(`Error loading translations (/i18n/${savedLang}.json):`, error);
      }
    });
  }
}
